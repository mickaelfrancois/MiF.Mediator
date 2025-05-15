using MiF.Mediator.Interfaces;
using System.Reflection;

namespace MiF.Mediator;

public class Mediator : IMediator
{
    private readonly IServiceFactory _serviceFactory;

    private readonly IValidationService? _validationService;



    /// Initializes a new instance of the <see cref="Mediator"/> class with the specified service factory.
    /// </summary>
    /// <param name="serviceFactory">The service factory used to resolve handler instances.</param>
    public Mediator(IServiceFactory serviceFactory)
    {
        _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class with the specified service factory and validation service.
    /// </summary>
    /// <param name="serviceFactory">The service factory used to resolve handler instances.</param>
    /// <param name="validationService">The validation service used to validate messages before handling.</param>
    public Mediator(IServiceFactory serviceFactory, IValidationService validationService)
    {
        _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }


    /// <summary>
    /// Sends a message asynchronously to the appropriate handler and returns a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response expected from the handler.</typeparam>
    /// <param name="message">The message implementing <see cref="IRequest{TResponse}"/> to be sent.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, containing the response of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the service factory is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the handler type is not known.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the handler returns null.</exception>
    public Task<TResponse> SendMessageAsync<TResponse>(IRequest<TResponse> message, CancellationToken cancellationToken = default)
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

        _validationService?.Validate(message);

        object? response = method.Invoke(instance, [message, cancellationToken]);

        return response == null ? throw new ArgumentNullException($"{instance.GetType().Name} returned null") : (Task<TResponse>)response;
    }
}