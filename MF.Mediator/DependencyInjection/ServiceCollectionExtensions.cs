using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiF.Mediator.Interfaces;
using System.Reflection;

namespace MiF.Mediator.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSimpleMediator(this IServiceCollection services)
        => services.AddSimpleMediator(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && !IsSystemAssembly(a)));

    public static IServiceCollection AddSimpleMediator(this IServiceCollection services, params Assembly[] assemblies)
        => services.AddSimpleMediator((IEnumerable<Assembly>)assemblies);

    public static IServiceCollection AddSimpleMediator(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        Assembly[] toScan = (assemblies as Assembly[] ?? assemblies.ToArray())
                        .Where(a => !IsSystemAssembly(a))
                        .Distinct()
                        .ToArray();

        services.TryAddScoped<IServiceFactory>(sp => new ServiceFactory(sp.GetService));
        services.TryAddScoped<IMediator, Mediator>();

        ReflectionUtilities.AddRequiredServices(services);
        ReflectionUtilities.AddSimpleMediatorClasses(services, toScan);
        ReflectionUtilities.AddSimpleMediatorPreProcessor(services, toScan);

        return services;
    }

    private static bool IsSystemAssembly(Assembly assembly)
    {
        string? name = assembly.GetName().Name;

        if (string.IsNullOrEmpty(name))
            return true;

        return name.StartsWith("System", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Windows", StringComparison.OrdinalIgnoreCase);
    }
}