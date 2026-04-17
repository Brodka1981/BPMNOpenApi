using BpmDomain.Handlers.Interfaces;

namespace BpmDomain.Factories.Interfaces;

public interface IServiceFactory
{
    public IServiceFieldHandler FieldResolve(string operationType);
    public IServiceTaskHandler TaskResolve(string operationType);
}