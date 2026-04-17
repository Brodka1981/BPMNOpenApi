using BpmApplication.Commands;
using BpmApplication.Handlers.Interfaces;

namespace BpmApplication.Handlers;

public class ExecuteActionHandler : ICommandHandler<ExecuteActionCommand, WorkflowActionResponse>
{
    public Task<WorkflowActionResponse> HandleAsync(ExecuteActionCommand cmd, CancellationToken ct = default)
    {
        var response = new WorkflowActionResponse
        {        
             
            Success = true,
            Message = $"Action {cmd.ActionName} executed"
        };

        return Task.FromResult(response);
    }
}