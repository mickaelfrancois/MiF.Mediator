using MiF.Mediator.Interfaces;

namespace MiF.Mediator;

public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand> where TCommand : IRequest<Unit>
{
    public async Task<Unit> HandleAsync(TCommand message, CancellationToken cancellationToken)
    {
        await HandleCommandAsync(message, cancellationToken);
        return Unit.Result;
    }

    protected abstract Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);
}
