using BpmInfrastructure.Models;

namespace BpmInfrastructure.Handlers.Interfaces;
public interface IServiceTaskHandler
{
    string TaskType { get; }

    Task ExecuteAsync(TaskDefinition task, Dictionary<string, object?> variables);
}