using MiF.Mediator.Interfaces;
using System.Reflection;

namespace MiF.Mediator;

public class Mediator(IServiceFactory serviceFactory) : IMediator
{
    private readonly IServiceFactory _serviceFactory = serviceFactory;

    public Task<TResponse> HandleAsync<TResponse>(IRequest<TResponse> message, CancellationToken cancellationToken = default)
    {
        Type targetType = message.GetType();
        Type targetHandler = typeof(IRequestProcessor<,>).MakeGenericType(targetType, typeof(TResponse));
        object instance = _serviceFactory.GetInstance(targetHandler);

        Task<TResponse> result = InvokeInstanceAsync(instance, message, targetHandler, cancellationToken);

        return result;
    }

    private Task<TResponse> InvokeInstanceAsync<TResponse>(object instance, IRequest<TResponse> message, Type targetHandler, CancellationToken cancellationToken)
    {
        MethodInfo? method = instance.GetType()
                            .GetTypeInfo()
                            .GetMethod(nameof(IRequestProcessor<IRequest<TResponse>, TResponse>.HandleAsync)) ?? throw new ArgumentException($"{instance.GetType().Name} is not a known {targetHandler.Name}", instance.GetType().FullName);

        object? response = method.Invoke(instance, [message, cancellationToken]);

        return response == null ? throw new ArgumentNullException($"{instance.GetType().Name} returned null") : (Task<TResponse>)response;
    }
}
