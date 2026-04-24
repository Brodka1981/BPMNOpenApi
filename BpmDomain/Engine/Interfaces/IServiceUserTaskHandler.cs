using BpmDomain.Models;

namespace BpmDomain.Engine.Interfaces;
public interface IServiceUserTaskHandler
{
    string TaskType { get; }

    Task ExecuteAsync(UserTaskDefinition task, Dictionary<string, object?> variables);
}