using CsTsSModelConverter;
using CsTsSModelConverter.Options;

namespace CsTsModelConverterConsoleApplication
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CsTsModelConverter.GenerateCode(options =>
            {
                options.IndentType = IndentType.TwoSpaces;
                options.NullableCollections = false;
                options.NullableObjects = false;
                options.NullableStrings = false;
                options.SourcePath = @"..\..\Input";
                options.DestinationPath = @"..\..\Output";
            });
        }
    }
}