namespace MiF.Mediator.Interfaces;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit> where TCommand : IRequest<Unit>
{
}

public interface ICommandHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IRequest<TResponse>
{
}
