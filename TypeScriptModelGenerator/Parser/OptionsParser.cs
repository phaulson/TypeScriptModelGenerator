using System.Text.Json;
using System.Text.Json.Serialization;
using TypeScriptModelGenerator.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TypeScriptModelGenerator.Parser;

internal static class OptionsParser
{
    public static TypeScriptGeneratorOptions ParseYaml(string content)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<TypeScriptGeneratorOptions>(content);
    }

    public static TypeScriptGeneratorOptions ParseJson(string content)
    {
        return JsonSerializer.Deserialize<TypeScriptGeneratorOptions>(content, new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        }) ?? new TypeScriptGeneratorOptions();
    }
}