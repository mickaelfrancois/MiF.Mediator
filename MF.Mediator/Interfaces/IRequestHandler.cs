namespace MiF.Mediator.Interfaces;

public interface IRequestHandler<in TMessage, TResponse> where TMessage : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TMessage message, CancellationToken cancellationToken);
}
