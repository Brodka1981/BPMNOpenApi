using BpmInfrastructure.Models;

namespace BpmInfrastructure.Repository.Interfaces;

public interface IDefinitionRepository
{
    Task<string> GetDefinitionXmlAsync(long definitionId, string company, CancellationToken ct);

    Task<IEnumerable<WorkflowDefinitionInfo>> GetDefinitionsAsync(string? category, CancellationToken ct);
}