﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSharpTypescriptConverter.Data;
using CSharpTypescriptConverter.Helper;
using CSharpTypescriptConverter.Options;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTypescriptConverter;

public class TypeScriptGenerator
{
    private TypeScriptGeneratorOptions _options = new();
    private List<TypeScriptFile> _tsFiles = new();
    private TypeScriptFile _currentTsFile = new();

    public void Generate(Action<TypeScriptGeneratorOptions> configure)
    {
        configure(_options);
        Generate(_options);
    }

    public void Generate(TypeScriptGeneratorOptions options)
    {
        _options = options;

        if (!_options.SourcePath.EndsWith("\\"))
        {
            _options.SourcePath += "\\";
        }

        if (!_options.DestinationPath.EndsWith("\\"))
        {
            _options.DestinationPath += "\\";
        }

        var sw = new Stopwatch();
        sw.Start();

        var sourceFiles = Directory.GetFiles(_options.SourcePath, "*.cs", SearchOption.AllDirectories);

        foreach (var sourceFile in sourceFiles)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile));
            var root = (CompilationUnitSyntax) tree.GetRoot();
            var tsFile = new TypeScriptFile
            {
                Ignored = AttributeHelper.IsIgnored(root.AttributeLists),
                OriginalName = DirectoryHelper.GetFileName(sourceFile),
                ReplacedName = AttributeHelper.GetTypescriptName(root.AttributeLists),
                RelativePath = DirectoryHelper.GetMiddlePath(sourceFile, _options.SourcePath)
            };

            tsFile.FullPath = $"{_options.DestinationPath}{tsFile.RelativePath}";
            if (!tsFile.FullPath.EndsWith("\\")) tsFile.FullPath = $"{tsFile.FullPath}\\";
            tsFile.FullPath = $"{tsFile.FullPath}{tsFile.Name}.ts";
            _currentTsFile = tsFile;
                
            foreach (var nameSpace in root.Members.Cast<BaseNamespaceDeclarationSyntax>())
            {
                foreach (var member in nameSpace.Members)
                {
                    TypeScriptConvertible tsClass = member switch
                    {
                        ClassDeclarationSyntax mClass => CreateInterface(mClass),
                        EnumDeclarationSyntax mEnum => CreateEnum(mEnum),
                        _ => throw new ArgumentException()
                    };

                    tsFile.Members.Add(tsClass);
                }
            }

            _tsFiles.Add(tsFile);
        }

        UpdateFiles();
        DirectoryHelper.WriteToFile(_options.DestinationPath, _tsFiles
            .Where(f => !f.Ignored && f.Members.Any()));

        Cleanup();

        sw.Stop();
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms = {sw.ElapsedMilliseconds / 1000}s");
    }

    private TypeScriptInterface CreateInterface(TypeDeclarationSyntax syntax)
    {
        var baseType = (syntax.BaseList?.Types.FirstOrDefault()?.Type as IdentifierNameSyntax)?.Identifier.Text;
        if (baseType is not null && !_currentTsFile.PossibleImports.Contains(baseType))
        {
            _currentTsFile.PossibleImports.Add(baseType);
        }
            
        var tsInterface = new TypeScriptInterface
        {
            Ignored = AttributeHelper.IsIgnored(syntax.AttributeLists),
            OriginalName = syntax.Identifier.Text,
            ReplacedName = AttributeHelper.GetTypescriptName(syntax.AttributeLists),
            Indent = _options.Indent,
            Generics =
                syntax.TypeParameterList?.Parameters.Select(p => p.Identifier.Text).ToList() ??
                new List<string>(),
            Base = baseType
        };

        if (tsInterface.Ignored) return tsInterface;
            
        foreach (var field in syntax.Members.Where(field => !AttributeHelper.IsIgnored(field.AttributeLists)))
        {
            if (field is not PropertyDeclarationSyntax property) continue;
            tsInterface.Properties.Add(CreateProperty(property));
        }

        return tsInterface;
    }
        
    private TypeScriptProperty CreateProperty(PropertyDeclarationSyntax syntax)
    {
        var name = syntax.Identifier.Text;
        var accessors = syntax.AccessorList?.Accessors;
        var typeInfo = new TypeHelper().ParseType(syntax.Type, _options);
            
        var property = new TypeScriptProperty
        {
            OriginalName = string.Concat(name[0].ToString().ToLower(), name.Substring(1)),
            ReplacedName = AttributeHelper.GetTypescriptName(syntax.AttributeLists),
            Readonly = (accessors?.All(a => a.Keyword.Text != "set") ?? true) 
                       || accessors.Value.Any(a => a.Modifiers.Any(m => m.Text == "private")),
            Type = typeInfo.Name,
            Optional = typeInfo.Optional
        };
            
        _currentTsFile.PossibleImports.AddRange(typeInfo.PossibleImports);
        _currentTsFile.PossibleImports = _currentTsFile.PossibleImports.Distinct().ToList();
            
        return property;
    }

    private TypeScriptEnum CreateEnum(EnumDeclarationSyntax syntax)
    {
        var tsEnum = new TypeScriptEnum
        {
            OriginalName = syntax.Identifier.Text,
            ReplacedName = AttributeHelper.GetTypescriptName(syntax.AttributeLists),
            Indent = _options.Indent
        };

        foreach (var enumField in syntax.Members)
        {
            int? value = null;
            if (enumField.EqualsValue?.Value is LiteralExpressionSyntax {Token.Value: int val}) value = val;
            tsEnum.Fields.Add(enumField.Identifier.Text, value);
        }

        return tsEnum;
    }

    private void UpdateFiles()
    {
        var members = _tsFiles.SelectMany(f => f.Members).ToList();
        foreach (var tsFile in _tsFiles)
        {
            foreach (var member in tsFile.Members)
            {
                if (member is not TypeScriptInterface tsInterface) return;

                if (tsInterface.Base is not null)
                {
                    tsInterface.Base = members.FirstOrDefault(c => c.OriginalName == tsInterface.Base)?.Name ??
                                       tsInterface.Base;
                }

                foreach (var property in tsInterface.Properties)
                {
                    property.Type = members.FirstOrDefault(c => c.OriginalName == property.Type)?.Name ??
                                    property.Type;
                }
            }

            foreach (var possibleImport in tsFile.PossibleImports.Where(possibleImport =>
                         tsFile.Members.All(m => m.OriginalName != possibleImport)))
            {
                var fileToImport =
                    _tsFiles.FirstOrDefault(f => f.Members.Any(m => m.OriginalName == possibleImport));
                if (fileToImport == null) continue;

                var importPath =
                    $"{DirectoryHelper.CalculateRelativePath(tsFile.RelativePath, fileToImport.RelativePath)}/{fileToImport.Name}";

                var actualImport = members.FirstOrDefault(c => c.OriginalName == possibleImport)?.Name ??
                                   possibleImport;

                if (tsFile.Imports.ContainsKey(importPath) &&
                    !tsFile.Imports[importPath].Contains(actualImport))
                {
                    tsFile.Imports[importPath].Add(actualImport);
                }
                else
                {
                    tsFile.Imports.Add(importPath, new List<string> {actualImport});
                }
            }
        }
    }

    private void Cleanup()
    {
        _options = new TypeScriptGeneratorOptions();
        _tsFiles = new List<TypeScriptFile>();
        _currentTsFile = new TypeScriptFile();
    }
}