using MiF.Mediator.Interfaces;

namespace MiF.Mediator;

public abstract class QueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse> where TQuery : IRequest<TResponse>
{
    public Task<TResponse> HandleAsync(TQuery message, CancellationToken cancellationToken)
    {
        return HandleQueryAsync(message, cancellationToken);
    }

    protected abstract Task<TResponse> HandleQueryAsync(TQuery query, CancellationToken cancellationToken);
}
