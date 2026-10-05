using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TaskManager.Application;

public static class DependencyInjection
{
    private const string HandlerSuffix = "Handler";

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.TryAddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssembly(assembly);

        foreach (var handlerType in assembly.GetTypes().Where(IsHandler))
            services.AddScoped(handlerType);

        return services;
    }

    private static bool IsHandler(Type type) =>
        type is { IsClass: true, IsAbstract: false, IsPublic: true }
        && type.Name.EndsWith(HandlerSuffix, StringComparison.Ordinal);
}
