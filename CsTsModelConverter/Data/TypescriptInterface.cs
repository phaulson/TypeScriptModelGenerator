using System.Collections.Generic;
using System.Linq;

namespace CsTsSModelConverter.Data
{
    public class TypescriptInterface : Transpilable
    {
        public string Indent { get; set; }
        public string Base { get; set; }
        public List<string> Parameters { get; set; } = new();
        public List<TypescriptProperty> Properties { get; } = new();

        public override string Code =>
            $"export interface {Name}{(Parameters.Any() ? $"<{string.Join(", ", Parameters)}>" : "")}{(Base is not null ? $" extends {Base}" : "")} {{\r\n{Indent}{string.Join($"\r\n{Indent}", Properties.Select(p => p.Code))}\r\n}}";
    }
}