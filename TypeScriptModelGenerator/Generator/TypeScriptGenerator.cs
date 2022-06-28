using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeScriptModelGenerator.Data;
using TypeScriptModelGenerator.Helper;
using TypeScriptModelGenerator.Options;
using TypeScriptModelGenerator.Parser;

namespace TypeScriptModelGenerator.Generator;

public class TypeScriptGenerator : ITypeScriptGenerator
{
    private readonly TypeScriptGeneratorOptions _options = new();
    private TypeScriptFile _currentTsFile = new();
    private List<TypeScriptFile> _tsFiles = new();

    public TypeScriptGenerator()
    {
    }

    public TypeScriptGenerator(TypeScriptGeneratorOptions options)
    {
        _options = options;
    }

    public TypeScriptGenerator(string filePath)
    {
        var extension = DirectoryHelper.GetFileExtension(filePath);
        var content = File.ReadAllText(filePath);

        _options = extension switch
        {
            "yaml" or "yml" => OptionsParser.ParseYaml(content),
            "json" => OptionsParser.ParseJson(content),
            _ => throw new ArgumentException($"Unknown file extension: {extension}")
        };
    }

    public ITypeScriptGenerator WithSourceDirectory(string sourceDirectory)
    {
        _options.SourceDirectory = sourceDirectory;
        return this;
    }

    public ITypeScriptGenerator WithDestinationDirectory(string destinationDirectory)
    {
        _options.DestinationDirectory = destinationDirectory;
        return this;
    }

    public ITypeScriptGenerator WithIndentType(TypeScriptIndentType indentType)
    {
        _options.IndentType = indentType;
        return this;
    }

    public ITypeScriptGenerator WithNullableConvert(TypeScriptNullableConvert convert)
    {
        _options.NullableConvert = convert;
        return this;
    }

    public ITypeScriptGenerator WithNestedNullableConvert(TypeScriptNestedNullableConvert convert)
    {
        _options.NestedNullableConvert = convert;
        return this;
    }

    public ITypeScriptGenerator WithDateConvert(TypeScriptDateConvert convert)
    {
        _options.DateConvert = convert;
        return this;
    }

    public ITypeScriptGenerator WithAdditionalFiles(IList<TypeScriptAdditionalFile> files)
    {
        _options.AdditionalFiles = files;
        return this;
    }

    public ITypeScriptGenerator AddAdditionalFile(TypeScriptAdditionalFile file)
    {
        _options.AdditionalFiles.Add(file);
        return this;
    }

    public void Generate()
    {
        if (!_options.SourceDirectory.EndsWith("\\")) _options.SourceDirectory += "\\";

        if (!_options.DestinationDirectory.EndsWith("\\")) _options.DestinationDirectory += "\\";

        var sourceFiles = Directory.GetFiles(_options.SourceDirectory, "*.cs", SearchOption.AllDirectories);
        foreach (var sourceFile in sourceFiles)
        {
            var tsFile = CreateFile(sourceFile, DirectoryHelper.GetMiddlePath(sourceFile, _options.SourceDirectory));
            _tsFiles.Add(tsFile);
        }

        foreach (var sourceFile in _options.AdditionalFiles)
        {
            var tsFile = CreateFile(sourceFile.SourcePath, sourceFile.DestinationDirectory ?? "");
            _tsFiles.Add(tsFile);
        }

        UpdateFiles();
        DirectoryHelper.WriteToFile(_options.DestinationDirectory, _tsFiles
            .Where(f => !f.Ignored && f.Members.Any()));

        Cleanup();
    }

    private TypeScriptFile CreateFile(string sourceFile, string destinationPath)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile));
        var root = (CompilationUnitSyntax) tree.GetRoot();
        var tsFile = new TypeScriptFile
        {
            Ignored = AttributeParser.IsIgnored(root.AttributeLists),
            OriginalName = DirectoryHelper.GetFileName(sourceFile),
            ReplacedName = AttributeParser.GetTypescriptName(root.AttributeLists),
            RelativePath = destinationPath
        };

        tsFile.FullPath = $"{_options.DestinationDirectory}{tsFile.RelativePath}";
        if (!tsFile.FullPath.EndsWith("\\")) tsFile.FullPath = $"{tsFile.FullPath}\\";
        tsFile.FullPath = $"{tsFile.FullPath}{tsFile.Name}.ts";
        _currentTsFile = tsFile;

        foreach (var nameSpace in root.Members.Cast<BaseNamespaceDeclarationSyntax>())
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

        return tsFile;
    }

    private TypeScriptInterface CreateInterface(TypeDeclarationSyntax syntax)
    {
        var baseTypeSyntax = syntax.BaseList?.Types.FirstOrDefault()?.Type;
        string? baseType = null;

        if (baseTypeSyntax is not null)
        {
            var typeInfo = new TypeParser().ParseType(baseTypeSyntax, _options);
            baseType = typeInfo.Name;

            _currentTsFile.PossibleImports.AddRange(typeInfo.PossibleImports);
            _currentTsFile.PossibleImports = _currentTsFile.PossibleImports.Distinct().ToList();
        }

        var tsInterface = new TypeScriptInterface
        {
            Ignored = AttributeParser.IsIgnored(syntax.AttributeLists),
            OriginalName = syntax.Identifier.Text,
            ReplacedName = AttributeParser.GetTypescriptName(syntax.AttributeLists),
            Indent = _options.Indent,
            Generics =
                syntax.TypeParameterList?.Parameters.Select(p => p.Identifier.Text).ToList() ??
                new List<string>(),
            Base = baseType
        };

        if (tsInterface.Ignored) return tsInterface;

        foreach (var field in syntax.Members.Where(field => !AttributeParser.IsIgnored(field.AttributeLists)))
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
        var typeInfo = new TypeParser().ParseType(syntax.Type, _options);

        var property = new TypeScriptProperty
        {
            OriginalName = string.Concat(name[0].ToString().ToLower(), name.Substring(1)),
            ReplacedName = AttributeParser.GetTypescriptName(syntax.AttributeLists),
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
            ReplacedName = AttributeParser.GetTypescriptName(syntax.AttributeLists),
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
                    tsInterface.Base = members.FirstOrDefault(c => c.OriginalName == tsInterface.Base)?.Name ??
                                       tsInterface.Base;

                foreach (var property in tsInterface.Properties)
                    property.Type = members.FirstOrDefault(c => c.OriginalName == property.Type)?.Name ??
                                    property.Type;
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
                    tsFile.Imports[importPath].Add(actualImport);
                else
                    tsFile.Imports.Add(importPath, new List<string> {actualImport});
            }
        }
    }

    private void Cleanup()
    {
        _tsFiles = new List<TypeScriptFile>();
        _currentTsFile = new TypeScriptFile();
    }
}