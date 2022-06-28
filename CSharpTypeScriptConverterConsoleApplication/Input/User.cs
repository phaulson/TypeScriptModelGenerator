using System;
using System.Collections.Generic;
using CSharpTypeScriptConverter.Attributes;
using CSharpTypeScriptConverterConsoleApplication.Input.Base;

[assembly: TypeScriptModel(Name = "IUser")]

namespace CSharpTypeScriptConverterConsoleApplication.Input;

[TypeScriptModel(Name = "IUser")]
public class User : BaseEntity<int>
{
    public string Name { get; set; } = null!;
    public DateTime? Birthday { get; set; }
    public List<Role> Roles { get; set; } = new();
}