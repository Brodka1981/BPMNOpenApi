using BpmInfrastructure.Models;
using BpmInfrastructure.Results;

namespace BpmInfrastructure.Repository.Interfaces;

public interface ISqlCommonRepository
{
    Task<GetProcessDefinitionSqlResult?> GetProcessDefinitionAsync(GetProcessDefinitionSqlParms parms, CancellationToken ct);
    Task<GetProcessInstanceSqlResult?> GetProcessInstanceAsync(GetProcessInstanceSqlParms parms, CancellationToken ct);
    Task<GetVariableSqlResult?> GetVariablesAsync(GetVariableSqlParms parms, CancellationToken ct);
    Task<bool> IsUserActivityCompleteAsync(IsUserActivityCompleteSqlParms parms, CancellationToken ct);
}