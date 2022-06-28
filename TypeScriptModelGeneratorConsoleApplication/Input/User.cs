using TypeScriptModelGenerator.Attributes;
using TypeScriptModelGeneratorConsoleApplication.Input.Base;

[assembly: TypeScriptModel(Name = "IUser")]

namespace TypeScriptModelGeneratorConsoleApplication.Input;

[TypeScriptModel(Name = "IUser")]
public class User : BaseEntity<int>
{
    public string Name { get; set; } = null!;
    public DateTime? Birthday { get; set; }
    public List<Role> Roles { get; set; } = new();
}