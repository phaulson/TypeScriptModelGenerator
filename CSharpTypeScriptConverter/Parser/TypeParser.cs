using System.Collections.Generic;
using System.Linq;
using CSharpTypeScriptConverter.Data;
using CSharpTypeScriptConverter.Options;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTypeScriptConverter.Parser;

internal class TypeParser
{
    private readonly Dictionary<string, string> _typeMappings = new()
    {
        {"int", "number"},
        {"float,", "number"},
        {"double", "number"},
        {"decimal", "number"},
        {"short", "number"},
        {"long", "number"},
        {"byte", "number"},
        {"bool", "boolean"},
        {"string", "string"},
        {"char", "string"},
        {"Guid", "string"},
        {"DateTime", "Date"},
        {"DateTimeOffset", "Date"},
    };

    private readonly HashSet<string> _listTypes = new()
    {
        "List", "IList", "ICollection", "IEnumerable", "IReadOnlyList", "IReadOnlyCollection"
    };

    private readonly HashSet<string> _mapTypes = new()
    {
        "Dictionary", "IDictionary", "IReadOnlyDictionary"
    };
        
    private readonly HashSet<string> _tupleTypes = new()
    {
        "Tuple", "ValueTuple", "KeyValuePair"
    };

    private readonly List<string> _possibleImports = new();
    private bool _optional;
    private TypeScriptGeneratorOptions _options = new();
        
    public TypeInformation ParseType(TypeSyntax type, TypeScriptGeneratorOptions options)
    {
        _possibleImports.Clear();
        _optional = false;
        _options = options;

        if (_options.DateConvert == TypeScriptDateConvert.String)
        {
            _typeMappings["Date"] = "string";
            _typeMappings["DateTime"] = "string";
        }
            
        var typeString = ParseTypeInternal(type, false);
        return new TypeInformation
        {
            Name = typeString,
            Optional = _optional,
            PossibleImports = _possibleImports
        };
    }

    private string ParseTypeInternal(TypeSyntax type, bool inner = true)
    {
        return type switch
        {
            PredefinedTypeSyntax syntax => _typeMappings.FirstOrDefault(t => t.Key == syntax.Keyword.Text).Value ?? "any",
            NullableTypeSyntax syntax => ParseNullableType(syntax, inner),
            IdentifierNameSyntax syntax => ParseIdentifierType(syntax),
            ArrayTypeSyntax syntax => ParseArrayType(syntax),
            GenericNameSyntax syntax => ParseGenericType(syntax),
            TupleTypeSyntax syntax => $"[{string.Join(", ", syntax.Elements.Select(e => ParseTypeInternal(e.Type)))}]",
            _ => "any"
        };
    }

    private string ParseNullableType(NullableTypeSyntax syntax, bool inner)
    {
        var type = ParseTypeInternal(syntax.ElementType);
        if (inner)
        {
            type = $"({type} | {(_options.NestedNullableConvert == TypeScriptNestedNullableConvert.Null ? "null" : "undefined")})";
        }
        else
        {
            if (_options.NullableConvert == TypeScriptNullableConvert.Optional)
            {
                _optional = true;
            }
            else
            {
                type += $" | {(_options.NullableConvert == TypeScriptNullableConvert.Null ? "null" : "undefined")}";
            }
        }
            
        return type;
    }
        
    private string ParseIdentifierType(SimpleNameSyntax syntax)
    {
        var identifier = syntax.Identifier.Text;
        var type = _typeMappings.FirstOrDefault(t => t.Key == identifier).Value ?? identifier;

        if (type != identifier) return type;
            
        if (!_possibleImports.Contains(type))
        {
            _possibleImports.Add(type);
        }

        return type;
    }
        
    private string ParseArrayType(ArrayTypeSyntax syntax)
    {
        if (syntax.ElementType is PredefinedTypeSyntax {Keyword: {Text: "byte"}})
        {
            return "string";
        }
            
        var type = ParseTypeInternal(syntax.ElementType);
        type += "[]";

        return type;
    }

    private string ParseGenericType(GenericNameSyntax syntax)
    {
        var identifier = syntax.Identifier.Text;
        var arguments = syntax.TypeArgumentList.Arguments;

        if (_listTypes.Contains(identifier))
        {
            return $"{ParseTypeInternal(arguments[0])}[]";
        }

        if (_mapTypes.Contains(identifier))
        {
            return $"Map<{ParseTypeInternal(arguments[0])}, {ParseTypeInternal(arguments[1])}>";
        }

        if (_tupleTypes.Contains(identifier))
        {
            return $"[{ParseTypeInternal(arguments[0])}, {ParseTypeInternal(arguments[1])}]";
        }

        if (!_possibleImports.Contains(identifier))
        {
            _possibleImports.Add(identifier);
        }

        return $"{identifier}<{string.Join(", ", arguments.Select(a => ParseTypeInternal(a)))}>";
    }
}