using CSharpTypescriptConverter;
using CSharpTypescriptConverter.Helper;
using CSharpTypescriptConverter.Options;

namespace CSharpTypeScriptConverterConsoleApplication
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            // var tsGeneratorOptions = new TypeScriptGeneratorOptions
            // {
            //     IndentType = IndentType.FourSpaces,
            //     SourcePath = @"..\..\Input",
            //     DestinationPath = @"..\..\Output"
            // };
            // new TypeScriptGenerator().Generate(tsGeneratorOptions);
            
            new TypeScriptGenerator().Generate(options =>
            {
                options.IndentType = IndentType.TwoSpaces;
                options.SourcePath = @"..\..\Input";
                options.DestinationPath = @"..\..\Output";
            });
        }
    }
}