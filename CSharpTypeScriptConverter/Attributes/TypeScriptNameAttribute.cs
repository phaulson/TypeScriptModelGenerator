using System;

namespace CSharpTypescriptConverter.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Assembly,
    Inherited = false, AllowMultiple = true)]
public class TypeScriptNameAttribute : Attribute
{
    private readonly string _name;
    public TypeScriptNameAttribute(string name)
    {
        _name = name;
    }
}