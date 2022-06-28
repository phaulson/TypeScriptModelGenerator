using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeScriptModelGenerator.Parser;

internal static class AttributeParser
{
    public static bool IsIgnored(SyntaxList<AttributeListSyntax> attributeLists)
    {
        var ignored = attributeLists
            .FirstOrDefault(attrList => attrList.Attributes.Any(attr => attr.Name.ToString() == "TypeScriptModel"))
            ?.Attributes.FirstOrDefault()?.ArgumentList?.Arguments
            .FirstOrDefault(arg => arg.NameEquals?.Name.Identifier.Text == "Ignored")?.Expression.ToString();

        return ignored is not null && bool.Parse(ignored);
    }

    public static string? GetTypescriptName(SyntaxList<AttributeListSyntax> attributeLists)
    {
        var name = attributeLists
            .FirstOrDefault(attrList => attrList.Attributes.Any(attr => attr.Name.ToString() == "TypeScriptModel"))
            ?.Attributes.FirstOrDefault()?.ArgumentList?.Arguments
            .FirstOrDefault(arg => arg.NameEquals?.Name.Identifier.Text == "Name")?.Expression.ToString().Trim('"');

        return !string.IsNullOrWhiteSpace(name) ? name : null;
    }
}