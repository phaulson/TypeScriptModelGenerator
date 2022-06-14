using System.Collections.Generic;
using System.Linq;
using CsTsSModelConverter.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsTsSModelConverter.Helper
{
    public static class TypeHelper
    {
        private static TypescriptFile TsFile { get; set; } = new();
        private static List<string> Generics { get; set; } = new();
        
        public static string ParseType(TypeSyntax type, TypescriptFile tsFile, List<string> generics)
        {
            TsFile = tsFile;
            Generics = generics;
            
            return ParseTypeInternal(type, false);
        }

        private static string ParseTypeInternal(TypeSyntax type, bool inner = true)
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

        private static string ParsePredefinedType(PredefinedTypeSyntax syntax)
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
        
        private static string ParseNullableType(NullableTypeSyntax syntax, bool inner)
        {
            var type = ParseTypeInternal(syntax.ElementType);
            type += " | null";

            if (inner)
            {
                type = $"({type})";
            }
            
            return type;
        }
        
        private static string ParseIdentifierType(SimpleNameSyntax syntax)
        {
            var identifier = syntax.Identifier.Text;
            var type = identifier is "DateTime" or "DateTimeOffset" ? "Date" : identifier;

            if (type == "Date" || Generics.Contains(type)) return type;
            
            if (!TsFile.PossibleImports.Contains(type))
            {
                TsFile.PossibleImports.Add(type);
            }

            return type;
        }
        
        private static string ParseArrayType(ArrayTypeSyntax syntax)
        {
            if (syntax.ElementType is PredefinedTypeSyntax {Keyword: {Text: "byte"}})
            {
                return "string";
            }
            
            var type = ParseTypeInternal(syntax.ElementType);
            type += "[]";

            return type;
        }

        private static string ParseGenericType(GenericNameSyntax syntax)
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
                    if (!TsFile.PossibleImports.Contains(identifier))
                    {
                        TsFile.PossibleImports.Add(identifier);
                    }

                    break;
            }

            return type;
        }
    }
}