using BpmInfrastructure.Results;

namespace BpmInfrastructure.Repository.Interfaces;

public interface ICompetenceDefinitionRepository
{
    Task<IEnumerable<GetCompetenceDefinitionSqlResult>> GetCompetenceDefinitionsAsync(
        string competenceType,
        string processDefinitionId,
        int tenantId,
        IReadOnlyCollection<string> participants);
}