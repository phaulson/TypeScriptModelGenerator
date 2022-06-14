using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsTsSModelConverter.Data;
using CsTsSModelConverter.Helper;
using CsTsSModelConverter.Options;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsTsSModelConverter
{
    public static class CsTsModelConverter
    {
        private static CsTsModelConverterOptions Options { get; set; } = new();
        public static void GenerateCode(Action<CsTsModelConverterOptions> configure)
        {
            var options = new CsTsModelConverterOptions();
            configure(options);
            Options = options;

            if (!Options.SourcePath.EndsWith("\\"))
            {
                Options.SourcePath += "\\";
            }

            if (!Options.DestinationPath.EndsWith("\\"))
            {
                Options.DestinationPath += "\\";
            }

            var sw = new Stopwatch();
            sw.Start();
            
            var sourceFiles = Directory.GetFiles(options.SourcePath, "*.cs", SearchOption.AllDirectories);
            var tsFiles = new List<TypescriptFile>();

            Parallel.ForEach(sourceFiles, sourceFile => 
            //foreach (var sourceFile in sourceFiles)
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile));
                var root = (CompilationUnitSyntax) tree.GetRoot();
                var tsFile = new TypescriptFile
                {
                    Name = DirectoryHelper.GetFileName(sourceFile),
                    RelativePath = DirectoryHelper.GetMiddlePath(sourceFile, Options.SourcePath)
                };
                
                tsFile.FullPath = $"{Options.DestinationPath}{tsFile.RelativePath}";
                if (!tsFile.FullPath.EndsWith("\\")) tsFile.FullPath = $"{tsFile.FullPath}\\";
                tsFile.FullPath = $"{tsFile.FullPath}{tsFile.Name}.ts";

                if (!tsFile.Ignored)
                {
                    foreach (var nameSpace in root.Members.Cast<NamespaceDeclarationSyntax>())
                    {
                        foreach (var member in nameSpace.Members)
                        {
                            tsFile.Members.Add(member switch
                            {
                                ClassDeclarationSyntax mClass => CreateInterface(mClass, tsFile),
                                EnumDeclarationSyntax mEnum => CreateEnum(mEnum),
                                _ => throw new ArgumentException()
                            });
                        }
                    }
                }

                tsFiles.Add(tsFile);
            });
            //}

            UpdateImports(tsFiles);
            DirectoryHelper.WriteToFile(Options.DestinationPath, tsFiles
                .Where(f => !f.Ignored));
            
            sw.Stop();
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms = {sw.ElapsedMilliseconds / 1000}s");
        }

        private static TypescriptInterface CreateInterface(TypeDeclarationSyntax mClass, TypescriptFile tsFile)
        {
            var baseType = (mClass.BaseList?.Types.FirstOrDefault()?.Type as IdentifierNameSyntax)?.Identifier.Text;
            if (baseType is not null && !tsFile.PossibleImports.Contains(baseType))
            {
                tsFile.PossibleImports.Add(baseType);
            }
            
            var tsInterface = new TypescriptInterface
            {
                Name = mClass.Identifier.Text,
                Indent = Options.Indent,
                Generics =
                    mClass.TypeParameterList?.Parameters.Select(p => p.Identifier.Text).ToList() ??
                    new List<string>(),
                Base = baseType
            };
            
            foreach (var field in mClass.Members)
            {
                if (field is PropertyDeclarationSyntax property)
                {
                    tsInterface.Properties.Add(CreateProperty(property, tsFile, tsInterface.Generics));
                }
            }

            return tsInterface;
        }
        
        private static TypescriptProperty CreateProperty(PropertyDeclarationSyntax syntax, TypescriptFile tsFile, List<string> generics)
        {
            var name = syntax.Identifier.Text;
            var accessors = syntax.AccessorList?.Accessors;
            var property = new TypescriptProperty
            {
                Name = string.Concat(name[0].ToString().ToLower(), name.Substring(1)),
                Readonly = (accessors?.All(a => a.Keyword.Text != "set") ?? true) 
                           || accessors.Value.Any(a => a.Modifiers.Any(m => m.Text == "private")),
                Type = TypeHelper.ParseType(syntax.Type, tsFile, generics)
            };
            
            return property;
        }

        private static TypescriptEnum CreateEnum(EnumDeclarationSyntax mEnum)
        {
            var tsEnum = new TypescriptEnum
            {
                Name = mEnum.Identifier.Text,
                Indent = Options.Indent
            };

            foreach (var enumField in mEnum.Members)
            {
                int? value = null;
                if (enumField.EqualsValue?.Value is LiteralExpressionSyntax {Token.Value: int val}) value = val;
                tsEnum.Fields.Add(enumField.Identifier.Text, value);
            }

            return tsEnum;
        }

        private static void UpdateImports(IReadOnlyCollection<TypescriptFile> tsFiles)
        {
            Parallel.ForEach(tsFiles, tsFile =>
            {
                foreach (var possibleImport in tsFile.PossibleImports.Where(possibleImport =>
                             tsFile.Members.All(i => i.Name != possibleImport)))
                {
                    var fileToImport = tsFiles.FirstOrDefault(f => f.Members.Any(i => i.Name == possibleImport));
                    if (fileToImport == null) continue;

                    var importPath =
                        $"{DirectoryHelper.CalculateRelativePath(tsFile.RelativePath, fileToImport.RelativePath)}/{fileToImport.Name}";
                    if (tsFile.Imports.ContainsKey(importPath) &&
                        !tsFile.Imports[importPath].Contains(possibleImport))
                    {
                        tsFile.Imports[importPath].Add(possibleImport);
                    }
                    else
                    {
                        tsFile.Imports.Add(importPath, new List<string> {possibleImport});
                    }
                }
            });
        }
    }
}