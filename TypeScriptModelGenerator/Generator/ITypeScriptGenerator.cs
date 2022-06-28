using TypeScriptModelGenerator.Options;

namespace TypeScriptModelGenerator.Generator;

public interface ITypeScriptGenerator
{
    ITypeScriptGenerator WithSourceDirectory(string sourceDirectory);
    ITypeScriptGenerator WithDestinationDirectory(string destinationDirectory);
    ITypeScriptGenerator WithIndentType(TypeScriptIndentType indentType);
    ITypeScriptGenerator WithNullableConvert(TypeScriptNullableConvert convert);
    ITypeScriptGenerator WithNestedNullableConvert(TypeScriptNestedNullableConvert convert);
    ITypeScriptGenerator WithDateConvert(TypeScriptDateConvert convert);
    ITypeScriptGenerator WithAdditionalFiles(IList<TypeScriptAdditionalFile> files);
    ITypeScriptGenerator AddAdditionalFile(TypeScriptAdditionalFile file);
    void Generate();
}