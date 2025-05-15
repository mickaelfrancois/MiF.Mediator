namespace MiF.Mediator.Interfaces;

public interface IMediator
{
    Task<TResponse> SendMessageAsync<TResponse>(IRequest<TResponse> message, CancellationToken cancellationToken = default);
}
