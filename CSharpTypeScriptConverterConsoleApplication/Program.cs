using CSharpTypeScriptConverter.Generator;
using CSharpTypeScriptConverter.Options;

namespace CSharpTypeScriptConverterConsoleApplication;

internal static class Program
{
    public static void Main(string[] args)
    {
        var generator = new TypeScriptGenerator()
            .WithSourceDirectory(@"..\..\Input")
            .WithDestinationDirectory(@"..\..\Output\")
            .WithIndentType(TypeScriptIndentType.TwoSpaces)
            .WithDateConvert(TypeScriptDateConvert.Date);
            
        generator.Generate();
    }
}