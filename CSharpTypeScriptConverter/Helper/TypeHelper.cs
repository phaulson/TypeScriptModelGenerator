using System.Collections.Generic;
using System.Linq;
using CSharpTypescriptConverter.Data;
using CSharpTypescriptConverter.Options;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTypescriptConverter.Helper
{
    public class TypeHelper
    {
        private readonly List<string> _possibleImports = new();
        private bool _optional;
        private TypeScriptGeneratorOptions _options = new();
        
        public TypeInformation ParseType(TypeSyntax type, TypeScriptGeneratorOptions options)
        {
            _possibleImports.Clear();
            _optional = false;
            _options = options;
            
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
                PredefinedTypeSyntax syntax => ParsePredefinedType(syntax),
                NullableTypeSyntax syntax => ParseNullableType(syntax, inner),
                IdentifierNameSyntax syntax => ParseIdentifierType(syntax),
                ArrayTypeSyntax syntax => ParseArrayType(syntax),
                GenericNameSyntax syntax => ParseGenericType(syntax),
                _ => ""
            };
        }

        private string ParsePredefinedType(PredefinedTypeSyntax syntax)
        {
            var type = syntax.Keyword.Text switch
            {
                "int" or "double" or "decimal" or "float" or "short" or "byte" => "number",
                "string" => "string",
                "bool" => "boolean",
                _ => "any"
            };

            return type;
        }
        
        private string ParseNullableType(NullableTypeSyntax syntax, bool inner)
        {
            var type = ParseTypeInternal(syntax.ElementType);
            if (inner)
            {
                type = $"({type} | {(_options.NestedNullableConvert == NestedNullableConvert.Null ? "null" : "undefined")})";
            }
            else
            {
                if (_options.NullableConvert == NullableConvert.Optional)
                {
                    _optional = true;
                }
                else
                {
                    type += $" | {(_options.NullableConvert == NullableConvert.Null ? "null" : "undefined")}";
                }
            }
            
            return type;
        }
        
        private string ParseIdentifierType(SimpleNameSyntax syntax)
        {
            var identifier = syntax.Identifier.Text;
            var type = identifier is "DateTime" or "DateTimeOffset" ? "Date" : identifier;

            if (type == "Date") return type;
            
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

            string type;
            switch (identifier)
            {
                case "List":
                    type = ParseTypeInternal(arguments[0]);
                    type += "[]";
                    break;
                case "Dictionary":
                    type = $"Map<{ParseTypeInternal(arguments[0])}, {ParseTypeInternal(arguments[1])}>";
                    break;
                default:
                    type = $"{identifier}<{string.Join(", ", arguments.Select(a => ParseTypeInternal(a)))}>";
                    if (!_possibleImports.Contains(identifier))
                    {
                        _possibleImports.Add(identifier);
                    }

                    break;
            }

            return type;
        }
    }
}