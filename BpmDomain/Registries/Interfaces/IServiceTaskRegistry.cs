using BpmInfrastructure.Handlers.Interfaces;

namespace BpmDomain.Registries.Interfaces;

public interface IServiceTaskRegistry
{
    public IServiceTaskHandler Resolve(string taskType);
}