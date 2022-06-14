using System.Collections.Generic;
using System.Linq;

namespace CsTsSModelConverter.Data
{
    public class TypescriptFile : TypescriptConvertible
    {
        public List<string> PossibleImports { get; set; } = new();
        public string FullPath { get; set; } = null!;
        public string RelativePath { get; set; } = null!;
        public Dictionary<string, List<string>> Imports { get; set; } = new();
        public List<TypescriptConvertible> Members { get; set; } = new();
        public bool Ignored { get; set; } = false;

        public override string Code => (Imports.Count > 0
                                           ? string.Join("\r\n",
                                                 Imports.Select(i =>
                                                     $"import {{ {string.Join(", ", i.Value)} }} from '{i.Key}'")) +
                                             "\r\n\r\n"
                                           : "") +
                                       string.Join("\r\n\r\n", Members.Select(i => i.Code));
    }
}