using TypeScriptModelGenerator.Generator;
using TypeScriptModelGenerator.Options;

var generator = new TypeScriptGenerator()
    .WithSourceDirectory(@"..\..\..\Input")
    .WithDestinationDirectory(@"..\..\..\Output")
    .WithIndentType(TypeScriptIndentType.TwoSpaces)
    .WithDateConvert(TypeScriptDateConvert.Date);

generator.Generate();