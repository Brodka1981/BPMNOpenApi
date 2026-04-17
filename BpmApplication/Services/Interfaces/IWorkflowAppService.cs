using BpmApplication.Commands;
using BpmApplication.DTO;
using BpmApplication.Results;

namespace BpmApplication.Services.Interfaces;

public interface IWorkflowAppService
{
    Task<Result<StartProcessResult>> StartProcessAsync(StartProcessCommand cmd, CancellationToken ct = default);
    Task<Result<GetContextResult>> GetContextAsync(GetContextCommand cmd, CancellationToken ct = default);
    Task<Result<IEnumerable<WorkflowDefinitionDto>>> GetDefinitionsAsync(GetDefinitionsCommand cmd, CancellationToken ct = default);
    Task<Result<IEnumerable<SearchProcessDto>>> SearchProcessAsync(SearchProcessCommand cmd, CancellationToken ct = default);
}