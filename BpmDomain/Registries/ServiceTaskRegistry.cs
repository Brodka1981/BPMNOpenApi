using BpmDomain.Engine.Interfaces;
using BpmDomain.Registries.Interfaces;
using BpmInfrastructure.Registries.Interfaces;

namespace BpmDomain.Registries;

public class ServiceTaskRegistry : IServiceTaskRegistry
{
    private readonly Dictionary<string, BpmInfrastructure.Handlers.Interfaces.IServiceTaskHandler> _handlers;

    public ServiceTaskRegistry(IEnumerable<BpmInfrastructure.Handlers.Interfaces.IServiceTaskHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.TaskType, h => h, StringComparer.OrdinalIgnoreCase);
    }

    public BpmInfrastructure.Handlers.Interfaces.IServiceTaskHandler Resolve(string taskType)
    {
        if (!_handlers.TryGetValue(taskType, out var handler))
            throw new InvalidOperationException($"ServiceTask handler non trovato per '{taskType}'");

        return handler;
    }
}

