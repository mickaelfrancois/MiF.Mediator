using MiF.Mediator.Interfaces;

namespace MiF.Mediator;

public abstract class EventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IRequest<Unit>
{
    public async Task<Unit> HandleAsync(TEvent message, CancellationToken cancellationToken)
    {
        await HandleEventAsync(message, cancellationToken);
        return Unit.Result;
    }

    protected abstract Task HandleEventAsync(TEvent @event, CancellationToken cancellationToken);
}