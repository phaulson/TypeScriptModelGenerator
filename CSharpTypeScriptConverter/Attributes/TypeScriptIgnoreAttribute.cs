using System;

namespace CSharpTypescriptConverter.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Assembly,
    Inherited = false, AllowMultiple = true)]
public class TypeScriptIgnoreAttribute : Attribute
{

}