using BpmDomain.Handlers.Interfaces;
using BpmDomain.Models;

namespace BpmDomain.Handlers;

public class ServiceTaskHandler : IServiceTaskHandler
{
    public string OperationType => nameof(ServiceTaskHandler);

    /// <summary>
    /// Execute base method
    /// </summary>
    /// <param name="taskDefinition"></param>
    /// <param name="variables"></param>
    /// <returns></returns>
    public virtual async Task ExecuteAsync(TaskDefinition taskDefinition, Dictionary<string, object?> variables)
    {
        //TODO
    }
}