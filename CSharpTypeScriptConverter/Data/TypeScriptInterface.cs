using System.Collections.Generic;
using System.Linq;

namespace CSharpTypescriptConverter.Data;

public class TypeScriptInterface : TypeScriptConvertible
{
    public string Indent { get; set; } = null!;
    public string? Base { get; set; }
    public List<string> Generics { get; set; } = new();
    public List<TypeScriptProperty> Properties { get; set; } = new();

    public override string Code =>
        $"export interface {Name}{(Generics.Any() ? $"<{string.Join(", ", Generics)}>" : "")}{(Base is not null ? $" extends {Base}" : "")} {{\r\n{Indent}{string.Join($"\r\n{Indent}", Properties.Where(p => !p.Ignored).Select(p => p.Code))}\r\n}}";
}