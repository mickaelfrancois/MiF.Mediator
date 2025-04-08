namespace MiF.Mediator.Interfaces;

public interface IRequestProcessor<in TMessage, TResponse> where TMessage : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TMessage message, CancellationToken cancellationToken);
}
