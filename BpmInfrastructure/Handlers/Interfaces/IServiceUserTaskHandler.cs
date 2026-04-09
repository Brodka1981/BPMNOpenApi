using BpmInfrastructure.Models;

namespace BpmInfrastructure.Handlers.Interfaces;
public interface IServiceUserTaskHandler
{
    string TaskType { get; }

    Task ExecuteAsync(UserTaskDefinition task, Dictionary<string, object?> variables);
}