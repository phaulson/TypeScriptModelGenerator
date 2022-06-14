using System.Collections.Generic;
using System.Linq;

namespace CsTsSModelConverter.Data
{
    public class TypescriptEnum : Transpilable
    {
        public string Indent { get; set; }
        public Dictionary<string, int?> Fields { get; } = new();

        public override string Code =>
            $"export enum {Name} {{\r\n{Indent}{string.Join($",\r\n{Indent}", Fields.Select(f => f.Key + (f.Value != null ? $" = {f.Value}" : "")))}\r\n}}";
    }
}