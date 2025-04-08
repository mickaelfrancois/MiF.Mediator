namespace MiF.Mediator.Interfaces;

public interface IEventHandler<in TEvent> : IRequestHandler<TEvent, Unit> where TEvent : IRequest<Unit>
{
}