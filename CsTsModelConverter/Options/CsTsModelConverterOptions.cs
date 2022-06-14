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
        public string SourcePath { get; set; } = null!;
        public string DestinationPath { get; set; } = Directory.GetCurrentDirectory();
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