using MiF.Mediator.Interfaces;

namespace MiF.Mediator;

public class ServiceFactory(ServiceFactoryDelegate _serviceFactoryDelegate) : IServiceFactory
{
    public object GetInstance(Type T)
    {
        return _serviceFactoryDelegate.Invoke(T);
    }
}
