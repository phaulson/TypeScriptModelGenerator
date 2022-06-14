using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsTsSModelConverter.Data;
using CsTsSModelConverter.Helper;
using CsTsSModelConverter.Options;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using YamlDotNet.Serialization.NamingConventions;

namespace CsTsSModelConverter
{
    public static class CsTsModelConverter
    {
        public static void GenerateCode(Action<CsTsModelConverterOptions> configure)
        {
            var options = new CsTsModelConverterOptions();
            configure?.Invoke(options);

            if (options.SourcePath == null)
            {
                return;
            }

            if (!options.SourcePath.EndsWith("\\"))
            {
                options.SourcePath += "\\";
            }
            if (!options.DestinationPath.EndsWith("\\"))
            {
                options.DestinationPath += "\\";
            }

            var ignoreFileOptions = options.IgnoreFilePath != null ? ParseIgnoreFileOptions(options) : new IgnoreFileOptions();

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
                    RelativePath = DirectoryHelper.GetMiddlePath(sourceFile, options.SourcePath),
                    Ignored = ignoreFileOptions.SourceFiles.Contains(sourceFile)
                };
                tsFile.FullPath = $"{options.DestinationPath}{tsFile.RelativePath}";
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
                                ClassDeclarationSyntax mClass => CreateInterface(mClass, tsFile, options),
                                EnumDeclarationSyntax mEnum => CreateEnum(mEnum, options),
                                _ => null
                            });
                        }
                    }
                }

                tsFiles.Add(tsFile);
            });
            //}

            UpdateImports(tsFiles);
            DirectoryHelper.WriteToFile(options.DestinationPath, tsFiles
                    .Where(f => !f.Ignored && !ignoreFileOptions.DestinationFiles.Contains(f.FullPath)));

            if (!options.Cleanup) return;
            
            var filesToIgnore = ignoreFileOptions.DestinationFiles
                    .Concat(tsFiles.Where(f => !f.Ignored).Select(f => f.FullPath)).ToList();
                
            DirectoryHelper.Cleanup(options.DestinationPath, options.DestinationPath, filesToIgnore);
        }

        private static IgnoreFileOptions ParseIgnoreFileOptions(CsTsModelConverterOptions options)
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var ignoreFileOptions = deserializer.Deserialize<IgnoreFileOptions>(File.ReadAllText(options.IgnoreFilePath))
                ?? new IgnoreFileOptions();
                
            ignoreFileOptions.SourceFiles = ignoreFileOptions.SourceFiles?.Select(f =>
            {
                var newF = f.Replace("/", "\\");
                if (newF.StartsWith("\\")) newF = newF.Substring(1);
                if (!newF.EndsWith(".cs")) newF = $"{newF}.cs";
                return $"{options.SourcePath}{newF}";
            }).ToList() ?? new List<string>();

            ignoreFileOptions.DestinationFiles = ignoreFileOptions.DestinationFiles?.Select(f =>
            {
                var newF = f.Replace("/", "\\");
                if (newF.StartsWith("\\")) newF = newF.Substring(1);
                return $"{options.DestinationPath}{newF}";
            }).ToList() ?? new List<string>();

            return ignoreFileOptions;
        }

        private static TypescriptInterface CreateInterface(TypeDeclarationSyntax mClass, TypescriptFile tsFile, 
            CsTsModelConverterOptions options)
        {
            var baseType = (mClass.BaseList?.Types.FirstOrDefault()?.Type as IdentifierNameSyntax)?.Identifier.Text;
            if (baseType is not null && !tsFile.PossibleImports.Contains(baseType))
            {
                tsFile.PossibleImports.Add(baseType);
            }
            
            var tsInterface = new TypescriptInterface
            {
                Name = mClass.Identifier.Text,
                Indent = options.Indent,
                Parameters =
                    mClass.TypeParameterList?.Parameters.Select(p => p.Identifier.Text).ToList() ??
                    new List<string>(),
                Base = baseType
            };

            foreach (var field in mClass.Members)
            {
                if (field is PropertyDeclarationSyntax property)
                {
                    tsInterface.Properties.Add(CreateProperty(property, options, tsFile,
                        tsInterface.Parameters));
                }
            }

            return tsInterface;
        }
        
        private static TypescriptProperty CreateProperty(PropertyDeclarationSyntax syntax, CsTsModelConverterOptions options,
            TypescriptFile tsFile, ICollection<string> generics)
        {
            var name = syntax.Identifier.Text;
            var accessors = syntax.AccessorList?.Accessors;
            var property = new TypescriptProperty
            {
                Name = string.Concat(name[0].ToString().ToLower(), name.Substring(1)),
                Readonly = (accessors?.All(a => a.Keyword.Text != "set") ?? true) 
                           || (accessors.Value.Any(a => a.Modifiers.Any(m => m.Text == "private"))),
                Type = TypeHelper.ParseType(syntax.Type, options, tsFile, generics)
            };
            
            return property;
        }

        private static TypescriptEnum CreateEnum(EnumDeclarationSyntax mEnum, CsTsModelConverterOptions options)
        {
            var tsEnum = new TypescriptEnum
            {
                Name = mEnum.Identifier.Text,
                Indent = options.Indent
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