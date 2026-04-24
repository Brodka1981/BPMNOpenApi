using BpmInfrastructure.Models;
using BpmInfrastructure.Results;

namespace BpmInfrastructure.Repository.Interfaces;

public interface IProcessDefinitionRepository
{
    Task<IEnumerable<WorkflowDefinitionInfo>> GetProcessDefinitionsAsync(string? category, CancellationToken ct);

    Task<GetProcessDefinitionSqlResult?> GetProcessDefinitionByTypeAsync(string type, int company, CancellationToken ct);
}