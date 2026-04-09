using BpmDomain.Models;
using BpmDomain.Results;

namespace BpmDomain.Engine.Interfaces;

public interface IWorkflowEngine
{
    ExecutionResult Start(WorkflowDefinition definition, Dictionary<string, object?> variables);

    ExecutionResult Execute(
        WorkflowDefinition definition,
        string currentNodeId,
        Dictionary<string, object?> variables,
        string? actionId = null);
}
