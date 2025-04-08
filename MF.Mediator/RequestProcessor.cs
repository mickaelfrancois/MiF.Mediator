using MiF.Mediator.Interfaces;

namespace MiF.Mediator;

public class RequestProcessor<TMessage, TResponse>(IServiceFactory serviceFactory) : IRequestProcessor<TMessage, TResponse> where TMessage : IRequest<TResponse>
{
    private readonly IEnumerable<IRequestHandler<TMessage, TResponse>> _messageHandlers = (IEnumerable<IRequestHandler<TMessage, TResponse>>)serviceFactory.GetInstance(typeof(IEnumerable<IRequestHandler<TMessage, TResponse>>));
    private readonly IEnumerable<IRequestPreProcessor<TMessage, TResponse>> _preProcessor = (IEnumerable<IRequestPreProcessor<TMessage, TResponse>>)serviceFactory.GetInstance(typeof(IEnumerable<IRequestPreProcessor<TMessage, TResponse>>));

    public Task<TResponse> HandleAsync(TMessage message, CancellationToken cancellationToken)
    {
        return RunMiddleware(message, HandleMessageAsync, cancellationToken);
    }

    private Task<TResponse> RunMiddleware(TMessage message, HandleRequestDelegate<TMessage, TResponse> handleMessageHandlerCall, CancellationToken cancellationToken)
    {
        HandleRequestDelegate<TMessage, TResponse>? next = null;

        next = _preProcessor.Reverse().Aggregate(handleMessageHandlerCall, (messageDelegate, processor) => (req, ct) => processor.RunAsync(req, messageDelegate, ct));
        return next.Invoke(message, cancellationToken);
    }

    private async Task<TResponse> HandleMessageAsync(TMessage messageObject, CancellationToken cancellationToken)
    {
        Type type = typeof(TMessage);

        if (!_messageHandlers.Any())
            throw new ArgumentException($"No handler of signature {typeof(IRequestHandler<,>).Name} was found for {typeof(TMessage).Name}", typeof(TMessage).FullName);

        if (typeof(IEvent).IsAssignableFrom(type))
        {
            IEnumerable<Task<TResponse>> tasks = _messageHandlers.Select(r => r.HandleAsync(messageObject, cancellationToken));
            TResponse? result = default;

            foreach (Task<TResponse>? task in tasks)
            {
                result = await task;
            }

            return result!;
        }

        if (typeof(IQuery<TResponse>).IsAssignableFrom(type) || typeof(ICommand).IsAssignableFrom(type) || typeof(ICommand<TResponse>).IsAssignableFrom(type))
            return await _messageHandlers.Single().HandleAsync(messageObject, cancellationToken);

        throw new ArgumentException($"{typeof(TMessage).Name} is not a known type of {typeof(IRequest<>).Name} - Query, Command or Event", typeof(TMessage).FullName);
    }
}