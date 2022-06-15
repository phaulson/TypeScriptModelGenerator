using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTypescriptConverter.Helper;

public static class AttributeHelper
{
    public static bool IsIgnored(SyntaxList<AttributeListSyntax> attributeLists)
    {
        return attributeLists.Any(attrList => attrList.Attributes.Any(attr => attr.Name.ToString() == "TypeScriptIgnore"));
    }

    public static string? GetTypescriptName(SyntaxList<AttributeListSyntax> attributeLists)
    {
        var name = attributeLists
            .FirstOrDefault(attrList => attrList.Attributes.Any(attr => attr.Name.ToString() == "TypeScriptName"))
            ?.Attributes.FirstOrDefault()?.ArgumentList?.Arguments.FirstOrDefault()?.Expression.ToString().Trim('"');
        return !string.IsNullOrWhiteSpace(name) ? name : null;
    }
}