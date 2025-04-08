namespace MiF.Mediator.Interfaces;

public delegate object ServiceFactoryDelegate(Type type);

public interface IServiceFactory
{
    object GetInstance(Type T);
}
