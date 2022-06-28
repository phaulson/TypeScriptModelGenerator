using System;
using CSharpTypeScriptConverter.Generator;
using CSharpTypeScriptConverter.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpTypeScriptConverter.Extensions;

public static class ServiceExtensions
{
    public static void AddTypeScriptGenerator(this IServiceCollection services,
        Action<TypeScriptGeneratorOptions> configure)
    {
        var options = new TypeScriptGeneratorOptions();
        configure(options);

        services.AddScoped<ITypeScriptGenerator, TypeScriptGenerator>(x => new TypeScriptGenerator(options));

        new TypeScriptGenerator(options).Generate();
    }
}