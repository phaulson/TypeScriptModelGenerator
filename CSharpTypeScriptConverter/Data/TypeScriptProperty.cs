namespace CSharpTypescriptConverter.Data;

public class TypeScriptProperty : TypeScriptConvertible
{
    public string Type { get; set; } = null!;
    public bool Optional { get; set; }
    public bool Readonly { get; set; }
    public override string Code => (Readonly ? "readonly " : "") + Name + (Optional ? "?" : "") + ": " + Type;
}