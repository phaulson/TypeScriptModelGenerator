using System.Collections.Generic;
using System.IO;

namespace CSharpTypeScriptConverter.Options;

public class TypeScriptGeneratorOptions
{
    public string SourceDirectory { get; set; } = null!;
    public string DestinationDirectory { get; set; } = Directory.GetCurrentDirectory();
    public TypeScriptIndentType IndentType { get; set; } = TypeScriptIndentType.TwoSpaces;
    public TypeScriptNullableConvert NullableConvert { get; set; } = TypeScriptNullableConvert.Optional;
    public TypeScriptNestedNullableConvert NestedNullableConvert { get; set; } = TypeScriptNestedNullableConvert.Null;
    public TypeScriptDateConvert DateConvert { get; set; } = TypeScriptDateConvert.Date;
    public IList<AdditionalFile> AdditionalFiles { get; set; } = new List<AdditionalFile>();

    public string Indent => IndentType switch
    {
        TypeScriptIndentType.Tab => "\t",
        TypeScriptIndentType.FourSpaces => "    ",
        TypeScriptIndentType.TwoSpaces => "  ",
        _ => ""
    };
}