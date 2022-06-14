using CsTsSModelConverter;
using CsTsSModelConverter.Options;

namespace CsTsModelConverterConsoleApplication
{
    public static class Program
    {
        public static void Main()
        {
            CsTsModelConverter.GenerateCode(options =>
            {
                options.IndentType = IndentType.TwoSpaces;
                options.SourcePath = @"..\..\Input";
                options.DestinationPath = @"..\..\Output";
            });
        }
    }
}