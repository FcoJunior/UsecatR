using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UsecatR.Abstractions;
using UsecatR.Runtime;

namespace UsecatR.DependencyInjection;

public static class UsecatRServiceCollectionExtensions
{
    public static IServiceCollection AddUsecator(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
            throw new ArgumentException("Provide at least one assembly to scan for use case handlers.", nameof(assemblies));

        services.AddScoped<IUsecatR, UsecatRBus>();

        RegisterHandlers(services, assemblies);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerInterface = typeof(IUseCaseHandler<,>);

        var types = assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToList();

        foreach (var impl in types)
        {
            foreach (var itf in impl.ImplementedInterfaces)
            {
                if (!itf.IsGenericType) continue;
                if (itf.GetGenericTypeDefinition() != handlerInterface) continue;

                services.AddScoped(itf, impl);
            }
        }
    }
}