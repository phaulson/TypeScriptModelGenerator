namespace TypeScriptModelGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Assembly,
    Inherited = false, AllowMultiple = true)]
public class TypeScriptModel : Attribute
{
    public bool Ignored;
    public string? Name;
}