using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MiF.Mediator.DependencyInjection;


public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSimpleMediator(this IServiceCollection services) => services.AddSimpleMediator(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic));


    public static IServiceCollection AddSimpleMediator(this IServiceCollection services, params Assembly[] assemblies) => services.AddSimpleMediator(assemblies.AsEnumerable());


    public static IServiceCollection AddSimpleMediator(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        ReflectionUtilities.AddRequiredServices(services);
        ReflectionUtilities.AddSimpleMediatorClasses(services, assemblies);
        ReflectionUtilities.AddSimpleMediatorPreProcessor(services, assemblies);

        return services;
    }
}
