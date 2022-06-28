using Microsoft.Extensions.DependencyInjection;
using TypeScriptModelGenerator.Generator;
using TypeScriptModelGenerator.Options;

namespace TypeScriptModelGenerator.Extensions;

public static class ServiceExtensions
{
    public static void AddTypeScriptGenerator(this IServiceCollection services,
        Action<TypeScriptGeneratorOptions> configure)
    {
        var options = new TypeScriptGeneratorOptions();
        configure(options);

        services.AddScoped<ITypeScriptGenerator, Generator.TypeScriptGenerator>(_ => new Generator.TypeScriptGenerator(options));

        new Generator.TypeScriptGenerator(options).Generate();
    }
}