using System.IO;

namespace CSharpTypescriptConverter.Options;

public class TypeScriptGeneratorOptions
{
    public string SourcePath { get; set; } = null!;
    public string DestinationPath { get; set; } = Directory.GetCurrentDirectory();
    public IndentType IndentType { get; set; } = IndentType.TwoSpaces;
    public NullableConvert NullableConvert { get; set; } = NullableConvert.Optional;
    public NestedNullableConvert NestedNullableConvert { get; set; } = NestedNullableConvert.Null;

    public string Indent => IndentType switch
    {
        IndentType.Tab => "\t",
        IndentType.FourSpaces => "    ",
        IndentType.TwoSpaces => "  ",
        _ => ""
    };
}