namespace MiF.Mediator.Interfaces;

public delegate Task<TResponse> HandleRequestDelegate<in TMessage, TResponse>(TMessage message, CancellationToken cancellationToken);

public interface IRequestPreProcessor<TMessage, TResponse> where TMessage : IRequest<TResponse>
{
    Task<TResponse> RunAsync(TMessage message, HandleRequestDelegate<TMessage, TResponse> next, CancellationToken cancellationToken);
}

