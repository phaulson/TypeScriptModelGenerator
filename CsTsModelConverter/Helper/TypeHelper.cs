using System.Collections.Generic;
using System.Linq;
using CsTsSModelConverter.Data;
using CsTsSModelConverter.Options;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsTsSModelConverter.Helper
{
    public static class TypeHelper
    {
        private static readonly CsTsModelConverterOptions DefaultOptions = new()
        {
            NullableStrings = false,
            NullableObjects = false,
            NullableCollections = false
        };

        public static string ParseType(TypeSyntax type, CsTsModelConverterOptions options, TypescriptFile tsFile, 
            ICollection<string> generics)
        {
            return ParseTypeInternal(type, tsFile, generics, options, false);
        }

        private static string ParseTypeInternal(TypeSyntax type, TypescriptFile tsFile, ICollection<string> generics, 
            CsTsModelConverterOptions options = null, bool inner = true)
        {
            return type switch
            {
                PredefinedTypeSyntax syntax => ParsePredefinedType(syntax, (options ?? DefaultOptions).NullableStrings),
                NullableTypeSyntax syntax => ParseNullableType(syntax, tsFile, generics, inner),
                IdentifierNameSyntax syntax => ParseIdentifierType(syntax, tsFile, generics, 
                    (options ?? DefaultOptions).NullableObjects),
                ArrayTypeSyntax syntax => ParseArrayType(syntax, tsFile, generics, 
                    (options ?? DefaultOptions).NullableCollections),
                GenericNameSyntax syntax => ParseGenericType(syntax, tsFile, generics, 
                    (options ?? DefaultOptions).NullableCollections),
                _ => ""
            };
        }

        private static string ParsePredefinedType(PredefinedTypeSyntax syntax, bool nullableStrings)
        {
            var type = syntax.Keyword.Text switch
            {
                "int" or "double" or "decimal" or "float" => "number",
                "string" => "string",
                "bool" => "boolean",
                _ => "any"
            };

            if (nullableStrings && type == "string")
            {
                type += " | null";
            }

            return type;
        }
        
        private static string ParseNullableType(NullableTypeSyntax syntax, TypescriptFile tsFile, 
            ICollection<string> generics, bool inner)
        {
            var type = ParseTypeInternal(syntax.ElementType, tsFile, generics);
            type += " | null";

            if (inner)
            {
                type = $"({type})";
            }
            
            return type;
        }
        
        private static string ParseIdentifierType(SimpleNameSyntax syntax, TypescriptFile tsFile, 
            ICollection<string> generics, bool nullableObjects)
        {
            var identifier = syntax.Identifier.Text;
            var type = identifier is "DateTime" or "DateTimeOffset" ? "Date" : identifier;

            if (type == "Date" || (generics?.Contains(type) ?? false)) return type;
            
            if (!tsFile.PossibleImports.Contains(type))
            {
                tsFile.PossibleImports.Add(type);
            }
            if (nullableObjects)
            {
                type += " | null";
            }

            return type;
        }
        
        private static string ParseArrayType(ArrayTypeSyntax syntax, TypescriptFile tsFile, 
            ICollection<string> generics, bool nullableCollections)
        {
            var type = ParseTypeInternal(syntax.ElementType, tsFile, generics);
            type += "[]";

            if (nullableCollections)
            {
                type += " | null";
            }

            return type;
        }
        
        private static string ParseGenericType(GenericNameSyntax syntax, TypescriptFile tsFile, 
            ICollection<string> generics, bool nullableCollections)
        {
            var identifier = syntax.Identifier.Text;
            var arguments = syntax.TypeArgumentList.Arguments;
            
            string type;
            switch (identifier)
            {
                case "List":
                    type = ParseTypeInternal(arguments[0], tsFile, generics);
                    type += "[]";
                    break;
                case "Dictionary":
                    type = $"{{ [key: string]: {ParseTypeInternal(arguments[1], tsFile, generics)} }}";
                    break;
                default:
                    type = $"{identifier}<{string.Join(", ", arguments.Select(a => ParseTypeInternal(a, tsFile, generics)))}>";
                    if (!tsFile.PossibleImports.Contains(identifier))
                    {
                        tsFile.PossibleImports.Add(identifier);
                    }
                    break;
            }

            if (nullableCollections)
            {
                type += " | null";
            }

            return type;
        }
    }
}