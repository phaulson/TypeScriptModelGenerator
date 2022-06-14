using System.IO;

namespace CsTsSModelConverter.Options
{
    public enum IndentType
    {
        TwoSpaces,
        FourSpaces,
        Tab
    }

    public class CsTsModelConverterOptions
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; } = Directory.GetCurrentDirectory();
        public string IgnoreFilePath { get; set; }
        public bool Cleanup { get; set; } = true;
        public bool NullableStrings { get; set; } = true;
        public bool NullableCollections { get; set; } = true;
        public bool NullableObjects { get; set; } = true;
        public IndentType IndentType { get; set; } = IndentType.TwoSpaces;

        public string Indent => IndentType switch
        {
            IndentType.Tab => "\t",
            IndentType.FourSpaces => "    ",
            IndentType.TwoSpaces => "  ",
            _ => ""
        };
    }
}