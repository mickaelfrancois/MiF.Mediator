namespace MiF.Mediator.Interfaces;

public interface ICommand<TResult> : IRequest<TResult>
{
}

public interface ICommand : IRequest<Unit>
{
}