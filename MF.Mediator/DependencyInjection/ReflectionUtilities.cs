using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiF.Mediator.Interfaces;
using System.Reflection;

namespace MiF.Mediator.DependencyInjection;
public static class ReflectionUtilities
{
    public static void AddSimpleMediatorClasses(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
    {
        assembliesToScan = (assembliesToScan as Assembly[] ?? assembliesToScan).Distinct().ToArray();

        Type[] openCommandAndQueryInterfaces =
        [
                typeof(IQueryHandler<,>),
                typeof(ICommandHandler<>),
                typeof(ICommandHandler<,>),
        ];

        Type[] openEventInterfaces =
        [
                typeof(IEventHandler<>),
        ];

        AddInterfacesAsTransient(openCommandAndQueryInterfaces, services, assembliesToScan, false);
        AddInterfacesAsTransient(openEventInterfaces, services, assembliesToScan, true);
    }


    public static IServiceCollection AddSimpleMediatorPreProcessor(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
    {
        var multiOpenInterfaces = new[]
        {
                typeof(IRequestPreProcessor<,>)
        };

        foreach (var multiOpenInterface in multiOpenInterfaces)
        {
            List<Type> concretions = [];

            foreach (TypeInfo? type in assembliesToScan.SelectMany(a => a.DefinedTypes))
            {
                IEnumerable<Type> interfaceTypes = type.FindInterfacesThatClose(multiOpenInterface).ToArray();

                if (!interfaceTypes.Any())
                    continue;

                if (type.IsConcrete())
                    concretions.Add(type);
            }

            // Always add every middleware
            foreach (Type c in concretions)
            {
                if (!c.IsGenericType)
                {
                    IEnumerable<Type> interfaceTypes = c.FindInterfacesThatClose(multiOpenInterface).ToArray();

                    foreach (var type in interfaceTypes)
                    {
                        services.AddTransient(type, c);
                    }
                }
                else
                {
                    services.AddTransient(multiOpenInterface, c);
                }

                // This is needed because MS DI doesn't support constrained items,
                // the service factory method registered in this class catches the argument exception and tries to resolve implemented types
                services.AddTransient(c);
            }
        }

        return services;
    }


    private static void AddInterfacesAsTransient(Type[] openMessageInterfaces, IServiceCollection services, IEnumerable<Assembly> assembliesToScan, bool addIfAlreadyExists)
    {
        foreach (Type openInterface in openMessageInterfaces)
        {
            List<Type> concretions = [];
            List<Type> interfaces = [];

            foreach (TypeInfo? type in assembliesToScan.SelectMany(a => a.DefinedTypes))
            {
                IEnumerable<Type> interfaceTypes = type.FindInterfacesThatClose(openInterface).ToArray();

                if (!interfaceTypes.Any())
                    continue;

                if (type.IsConcrete())
                    concretions.Add(type);

                foreach (Type interfaceType in interfaceTypes)
                {
                    if (interfaceType.GetInterfaces().Length != 0)
                    {
                        // Register the MessageHandler instead of ICommand/Query/EventHandler
                        interfaces.AddRange(interfaceType.GetInterfaces());
                    }
                    else
                    {
                        interfaces.Fill(interfaceType);
                    }
                }
            }

            foreach (Type? @interface in interfaces.Distinct())
            {
                List<Type> matches = concretions.Where(t => t.CanBeCastTo(@interface)).ToList();

                if (addIfAlreadyExists)
                {
                    matches.ForEach(match => services.AddTransient(@interface, match));
                }
                else
                {
                    if (matches.Count > 1)
                    {
                        matches.RemoveAll(m => !IsMatchingWithInterface(m, @interface));
                    }

                    matches.ForEach(match => services.TryAddTransient(@interface, match));
                }

                if (!@interface.IsOpenGeneric())
                {
                    AddConcretionsThatCouldBeClosed(@interface, concretions, services);
                }
            }
        }
    }

    private static bool IsMatchingWithInterface(Type handlerType, Type handlerInterface)
    {
        if (handlerType == null || handlerInterface == null)
            return false;

        if (handlerType.IsInterface)
        {
            if (handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments))
                return true;
        }
        else
        {
            return IsMatchingWithInterface(handlerType.GetInterface(handlerInterface.Name)!, handlerInterface);
        }

        return false;
    }

    private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
    {
        foreach (Type? type in concretions.Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
        {
            try
            {
                services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
            }
            catch (Exception)
            {
            }
        }
    }

    private static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
    {
        Type openInterface = closedInterface.GetGenericTypeDefinition();
        Type[] arguments = closedInterface.GenericTypeArguments;
        Type[] concreteArguments = openConcretion.GenericTypeArguments;

        return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
    }

    private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
    {
        if (pluggedType == null)
            return false;

        if (pluggedType == pluginType)
            return true;

        return pluginType.GetTypeInfo().IsAssignableFrom(pluggedType.GetTypeInfo());
    }

    public static bool IsOpenGeneric(this Type type)
    {
        return type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;
    }

    private static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
    {
        if (!pluggedType.IsConcrete())
            yield break;

        if (templateType.GetTypeInfo().IsInterface)
        {
            foreach (Type? interfaceType in pluggedType.GetTypeInfo().ImplementedInterfaces.Where(type => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == templateType))
            {
                yield return interfaceType;
            }
        }
        else if (pluggedType.GetTypeInfo().BaseType!.GetTypeInfo().IsGenericType && pluggedType.GetTypeInfo().BaseType!.GetGenericTypeDefinition() == templateType)
        {
            yield return pluggedType.GetTypeInfo().BaseType!;
        }

        if (pluggedType == typeof(object))
            yield break;

        if (pluggedType.GetTypeInfo().BaseType == typeof(object))
            yield break;

        foreach (Type interfaceType in pluggedType.GetTypeInfo().BaseType!.FindInterfacesThatClose(templateType))
        {
            yield return interfaceType;
        }
    }

    private static bool IsConcrete(this Type type)
    {
        return !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
    }

    private static void Fill<T>(this List<T> list, T value)
    {
        if (list.Contains(value))
            return;

        list.Add(value);
    }

    public static void AddRequiredServices(IServiceCollection services)
    {
        services.AddScoped<ServiceFactoryDelegate>(p => type =>
        {
            try
            {
                return p.GetService(type)!;
            }
            catch (ArgumentException)
            {
                // Let's assume it's a constrained generic type
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    Type serviceType = type.GenericTypeArguments.Single();
                    List<Type> serviceTypes = [];

                    foreach (ServiceDescriptor service in services)
                    {
                        if (serviceType.IsConstructedGenericType && serviceType.GetGenericTypeDefinition() == service.ServiceType)
                        {
                            try
                            {
                                Type closedImplType = service.ImplementationType!.MakeGenericType(serviceType.GenericTypeArguments);
                                serviceTypes.Add(closedImplType);
                            }
                            catch { }
                        }
                    }

                    services.Replace(new ServiceDescriptor(type, sp =>
                    {
                        return serviceTypes.Select(sp.GetService).ToArray();
                    }, ServiceLifetime.Transient));

                    var resolved = Array.CreateInstance(serviceType, serviceTypes.Count);

                    Array.Copy(serviceTypes.Select(p.GetService).ToArray(), resolved, serviceTypes.Count);

                    return resolved;
                }

                throw;
            }
        });

        services.AddScoped<IServiceFactory, ServiceFactory>();
        services.AddScoped(typeof(IRequestProcessor<,>), typeof(RequestProcessor<,>));
        services.AddScoped<IMediator, Mediator>();
    }
}