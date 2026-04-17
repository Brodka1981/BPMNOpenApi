using BpmDomain.Models;

namespace BpmDomain.Handlers.Interfaces;

public interface IServiceTaskHandler
{
    string OperationType { get; }
    Task ExecuteAsync(TaskDefinition taskDefinition, Dictionary<string, object?> variables);
}