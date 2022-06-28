using System.Collections.Generic;
using CSharpTypeScriptConverter.Options;

namespace CSharpTypeScriptConverter.Generator;

public interface ITypeScriptGenerator
{
    ITypeScriptGenerator WithSourceDirectory(string sourceDirectory);
    ITypeScriptGenerator WithDestinationDirectory(string destinationDirectory);
    ITypeScriptGenerator WithIndentType(TypeScriptIndentType indentType);
    ITypeScriptGenerator WithNullableConvert(TypeScriptNullableConvert convert);
    ITypeScriptGenerator WithNestedNullableConvert(TypeScriptNestedNullableConvert convert);
    ITypeScriptGenerator WithDateConvert(TypeScriptDateConvert convert);
    ITypeScriptGenerator WithAdditionalFiles(IList<AdditionalFile> files);
    ITypeScriptGenerator AddAdditionalFile(AdditionalFile file);
    void Generate();
}