using BpmApplication.Commands;
using BpmApplication.DTO;
using BpmApplication.Handlers;
using BpmApplication.Queries;
using BpmApplication.Queries.Interfaces;
using BpmApplication.Results;
using BpmApplication.Services.Interfaces;

namespace BpmApplication.Services;

public class WorkflowAppService : IWorkflowAppService
{
    private readonly StartProcessHandler _startProcessHandler;
    private readonly GetContextHandler _getContextHandler;
    private readonly GetDefinitionsHandler _getDefinitionsHandler;

    public WorkflowAppService(StartProcessHandler startProcessHandler, GetContextHandler getContextHandler, GetDefinitionsHandler getDefinitionsHandler)
    {
        _startProcessHandler = startProcessHandler;
        _getContextHandler = getContextHandler;
        _getDefinitionsHandler = getDefinitionsHandler;
    }

    /// <summary>
    /// Start Process Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Result<StartProcessResult>> StartProcessAsync(StartProcessCommand cmd, CancellationToken ct = default)
        => _startProcessHandler.HandleAsync(cmd, ct);

    /// <summary>
    /// Get Context Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Result<GetContextResult>> GetContextAsync(GetContextCommand cmd, CancellationToken ct = default)
        => _getContextHandler.HandleAsync(cmd, ct);

    //public async Task<WorkflowActionResponse> ExecuteActionAsync(WorkflowActionRequest request, string user)
    //{
    //    var cmd = new ExecuteActionCommand
    //    {
    //        ProcessId = request.ProcessId,
    //        ActionName = request.ActionName,
    //        User = user
    //    };

    //    return await _commandBus.SendAsync(cmd);
    //}

    //public async Task<bool> ValidateActionAsync(WorkflowValidateRequest request)
    //{
    //    var query = new ValidateActionQuery
    //    {
    //        ProcessId = request.ProcessId,
    //        ActionName = request.ActionName
    //    };

    //    return await _queryBus.SendAsync(query);
    //}

    //public async Task<Dictionary<string, object?>> GetVariablesAsync(Guid processId)
    //{
    //    var query = new GetVariablesQuery
    //    {
    //        ProcessId = processId
    //    };

    //    return await _queryBus.SendAsync(query);
    //}

    //public async Task<IEnumerable<PendingProcessDto>> GetPendingProcessesAsync(string user)
    //{
    //    var query = new GetPendingProcessesQuery
    //    {
    //        User = user
    //    };

    //    return await _queryBus.SendAsync(query);
    //}

    //public async Task<IEnumerable<WorkflowDefinitionDto>> GetDefinitionsAsync()
    //{
    //    var query = new GetDefinitionsQuery();
    //    return await _queryBus.SendAsync(query);
    //}

    //public async Task<string> GetBpmnAsync(string definitionId)
    //{
    //    var query = new GetBpmnQuery
    //    {
    //        DefinitionId = definitionId
    //    };

    //    return await _queryBus.SendAsync(query);
    //}

    public Task<Result<IEnumerable<WorkflowDefinitionDto>>> GetDefinitionsAsync(GetDefinitionsCommand cmd, CancellationToken ct = default)
        => _getDefinitionsHandler.HandleAsync(cmd, ct);
}