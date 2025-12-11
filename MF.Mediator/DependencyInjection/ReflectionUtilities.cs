using global::MiF.Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace MiF.Mediator.DependencyInjection;

public static class ReflectionUtilities
{
    public static void AddSimpleMediatorClasses(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
    {
        Assembly[] assemblies = (assembliesToScan as Assembly[] ?? assembliesToScan).Distinct().ToArray();

        Type[] openCommandAndQueryInterfaces = new[]
        {
            typeof(IQueryHandler<,>),
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
        };

        Type[] openEventInterfaces = new[]
        {
            typeof(IEventHandler<>),
        };

        AddInterfacesAsTransient(openCommandAndQueryInterfaces, services, assemblies, addIfAlreadyExists: false);
        AddInterfacesAsTransient(openEventInterfaces, services, assemblies, addIfAlreadyExists: true);
    }

    public static IServiceCollection AddSimpleMediatorPreProcessor(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
    {
        Type[] multiOpenInterfaces = new[]
        {
            typeof(IRequestPreProcessor<,>)
        };

        Type[] types = (assembliesToScan as Assembly[] ?? assembliesToScan)
            .SelectMany(a => a.DefinedTypes)
            .Select(t => t.AsType())
            .ToArray();

        foreach (Type multiOpenInterface in multiOpenInterfaces)
        {
            List<Type> concretions = types
                .Where(t => t.FindInterfacesThatClose(multiOpenInterface).Any())
                .Where(t => t.IsConcrete())
                .ToList();

            // Always add every middleware
            foreach (Type c in concretions)
            {
                if (!c.IsGenericType)
                {
                    Type[] interfaceTypes = c.FindInterfacesThatClose(multiOpenInterface).ToArray();
                    foreach (Type it in interfaceTypes)
                    {
                        services.AddTransient(it, c);
                    }
                }
                else
                {
                    services.AddTransient(multiOpenInterface, c);
                }

                // MS DI doesn't support constrained generic registrations; still register concrete for factory fallback
                services.AddTransient(c);
            }
        }

        return services;
    }

    private static void AddInterfacesAsTransient(Type[] openMessageInterfaces, IServiceCollection services, IEnumerable<Assembly> assembliesToScan, bool addIfAlreadyExists)
    {
        Type[] types = (assembliesToScan as Assembly[] ?? assembliesToScan)
            .SelectMany(a => a.DefinedTypes)
            .Select(t => t.AsType())
            .ToArray();

        foreach (Type openInterface in openMessageInterfaces)
        {
            List<Type> concretions = new();
            List<Type> interfaces = new();

            foreach (Type type in types)
            {
                Type[] interfaceTypes = type.FindInterfacesThatClose(openInterface).ToArray();
                if (!interfaceTypes.Any())
                    continue;

                if (type.IsConcrete())
                    concretions.Add(type);

                foreach (Type interfaceType in interfaceTypes)
                {
                    Type[] implemented = interfaceType.GetInterfaces();
                    if (implemented.Length != 0)
                    {
                        // Register the MessageHandler instead of ICommand/Query/EventHandler
                        interfaces.AddRange(implemented);
                    }
                    else
                    {
                        if (!interfaces.Contains(interfaceType))
                            interfaces.Add(interfaceType);
                    }
                }
            }

            foreach (Type @interface in interfaces.Distinct())
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
            return handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments);
        }
        else
        {
            Type? iface = handlerType.GetInterface(handlerInterface.Name);
            return iface != null && IsMatchingWithInterface(iface, handlerInterface);
        }
    }

    private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
    {
        foreach (Type type in concretions.Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
        {
            try
            {
                Type closed = type.MakeGenericType(@interface.GenericTypeArguments);
                services.TryAddTransient(@interface, closed);
            }
            catch
            {
                // ignore types that cannot be made
            }
        }
    }

    private static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
    {
        if (!openConcretion.IsGenericTypeDefinition)
            return false;

        Type openInterface = closedInterface.GetGenericTypeDefinition();
        Type[] arguments = closedInterface.GenericTypeArguments;
        Type[] concreteParameters = openConcretion.GetGenericArguments();

        return arguments.Length == concreteParameters.Length && openConcretion.CanBeCastTo(openInterface);
    }

    private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
    {
        if (pluggedType == null || pluginType == null)
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
        if (pluggedType == null || !pluggedType.IsConcrete())
            yield break;

        if (templateType.GetTypeInfo().IsInterface)
        {
            foreach (Type? interfaceType in pluggedType.GetInterfaces()
                         .Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == templateType))
            {
                yield return interfaceType;
            }
        }
        else
        {
            // Walk base type chain to find generic base types matching templateType
            Type? current = pluggedType;
            while (current != null && current != typeof(object))
            {
                Type? baseType = current.BaseType;
                if (baseType != null && baseType.GetTypeInfo().IsGenericType && baseType.GetGenericTypeDefinition() == templateType)
                    yield return baseType;

                current = baseType;
            }
        }
    }

    private static bool IsConcrete(this Type type)
    {
        return type != null && !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
    }

    // kept for compatibility but implemented simply
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
                // Handle constrained generic type requests for IEnumerable<T>
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    Type serviceType = type.GenericTypeArguments.Single();

                    // Collect descriptors that match the open generic service signature
                    List<ServiceDescriptor> matchingDescriptors = services
                        .Where(sd => sd.ServiceType.IsGenericType &&
                                     sd.ServiceType.GetGenericTypeDefinition() == serviceType.GetGenericTypeDefinition())
                        .ToList();

                    List<object?> instances = new();
                    foreach (ServiceDescriptor? sd in matchingDescriptors)
                    {
                        try
                        {
                            if (sd.ImplementationType != null && sd.ImplementationType.IsGenericTypeDefinition)
                            {
                                Type closedImpl = sd.ImplementationType.MakeGenericType(serviceType.GenericTypeArguments);
                                instances.Add(p.GetService(closedImpl));
                            }
                            else if (sd.ImplementationType != null)
                            {
                                instances.Add(p.GetService(sd.ImplementationType));
                            }
                            else if (sd.ImplementationInstance != null)
                            {
                                instances.Add(sd.ImplementationInstance);
                            }
                            else if (sd.ImplementationFactory != null)
                            {
                                instances.Add(sd.ImplementationFactory(p));
                            }
                        }
                        catch
                        {
                            // ignore individual failures
                        }
                    }

                    Array resolved = Array.CreateInstance(serviceType, instances.Count);
                    Array.Copy(instances.Select(i => i).ToArray(), resolved, instances.Count);
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