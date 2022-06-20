using System.Collections.Generic;
using System.Linq;

namespace CSharpTypescriptConverter.Data;

internal class TypeScriptFile : TypeScriptConvertible
{
    public List<string> PossibleImports { get; set; } = new();
    public string FullPath { get; set; } = null!;
    public string RelativePath { get; set; } = null!;
    public Dictionary<string, List<string>> Imports { get; set; } = new();
    public List<TypeScriptConvertible> Members { get; set; } = new();

    public override string Code => (Imports.Count > 0
                                       ? string.Join("\r\n",
                                             Imports.Select(i =>
                                                 $"import {{ {string.Join(", ", i.Value)} }} from '{i.Key}'")) +
                                         "\r\n\r\n"
                                       : "") +
                                   string.Join("\r\n\r\n", Members.Where(i => !i.Ignored).Select(i => i.Code));
}