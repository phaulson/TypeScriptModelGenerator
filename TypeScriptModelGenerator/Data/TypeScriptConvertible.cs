namespace TypeScriptModelGenerator.Data;

internal abstract class TypeScriptConvertible
{
    public bool Ignored { get; set; }
    public string OriginalName { get; set; } = null!;
    public string? ReplacedName { get; set; }
    public string Name => ReplacedName ?? OriginalName;
    public virtual string Code => "";
}