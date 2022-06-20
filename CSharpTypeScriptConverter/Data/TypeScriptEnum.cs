using System.Collections.Generic;
using System.Linq;

namespace CSharpTypescriptConverter.Data;

internal class TypeScriptEnum : TypeScriptConvertible
{
    public string Indent { get; set; } = null!;
    public Dictionary<string, int?> Fields { get; set; } = new();

    public override string Code =>
        $"export enum {Name} {{\r\n{Indent}{string.Join($",\r\n{Indent}", Fields.Select(f => f.Key + (f.Value != null ? $" = {f.Value}" : "")))}\r\n}}";
}