using BpmInfrastructure.Handlers.Interfaces;

namespace BpmInfrastructure.Registries.Interfaces;

public interface IServiceUserTaskRegistry
{
    IServiceUserTaskHandler Resolve(string taskType);
}