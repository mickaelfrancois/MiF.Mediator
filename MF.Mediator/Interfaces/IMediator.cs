namespace MiF.Mediator.Interfaces;

public interface IMediator
{
    Task<TResponse> HandleAsync<TResponse>(IRequest<TResponse> message, CancellationToken cancellationToken = default);
}
