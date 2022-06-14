using System.Collections.Generic;
using System.Linq;

namespace CsTsSModelConverter.Data
{
    public class TypescriptFile : Transpilable
    {
        public List<string> PossibleImports { get; } = new();
        public string FullPath { get; set; }
        public string RelativePath { get; set; }
        public Dictionary<string, List<string>> Imports { get; } = new();
        public List<Transpilable> Members { get; } = new();
        public bool Ignored { get; set; }
        public override string Code => (Imports.Count > 0
                                           ? string.Join("\r\n",
                                               Imports.Select(i => $"import {{ {string.Join(", ", i.Value)} }} from '{i.Key}'")) + "\r\n\r\n"
                                           : "") +
                                       string.Join("\r\n\r\n", Members.Select(i => i.Code));
    }
}