using System.Collections.Generic;

namespace CsTsSModelConverter.Options
{
    public class IgnoreFileOptions
    {
        public List<string> SourceFiles { get; set; } = new();
        public List<string> DestinationFiles { get; set; } = new();
    }
}