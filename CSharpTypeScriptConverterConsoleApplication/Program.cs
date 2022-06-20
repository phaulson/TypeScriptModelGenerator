using System.Collections.Generic;
using CSharpTypescriptConverter;
using CSharpTypescriptConverter.Options;

namespace CSharpTypeScriptConverterConsoleApplication
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            // new TypeScriptGenerator().Generate(options =>
            // {
            //     options.IndentType = TypeScriptIndentType.TwoSpaces;
            //     options.SourceDirectory = @"..\..\Input";
            //     options.DestinationDirectory = @"..\..\Output\";
            //     options.AdditionalFiles = new List<AdditionalFile>
            //     {
            //         new()
            //         {
            //             SourceDirectory = @"..\..\..\CSharpTypeScriptConverter\Data\TypeScriptConvertible.cs",
            //             DestinationDirectory = "TypeScript"
            //         }
            //     };
            // });
            
            new TypeScriptGenerator().Generate(@"..\..\Config\config.json");
        }
    }
}