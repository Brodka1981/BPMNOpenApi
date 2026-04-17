using BpmDomain.Models;

namespace BpmDomain.Engine.Interfaces;
public interface IServiceTaskHandler
{
    string TaskType { get; }

    Task ExecuteAsync(TaskDefinition task, Dictionary<string, object?> variables);
}