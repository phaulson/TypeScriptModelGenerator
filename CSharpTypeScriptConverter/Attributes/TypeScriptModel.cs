using System;

namespace CSharpTypescriptConverter.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Assembly,
    Inherited = false, AllowMultiple = true)]
public class TypeScriptModel : Attribute
{
    public string? Name;
    public bool Ignored;
}