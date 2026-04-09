using BpmInfrastructure.Common;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository.Interfaces;
using BpmInfrastructure.Results;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text;

namespace BpmInfrastructure.Repository;

public class SqlCompetenceDefinitionRepository : SqlRepository, ICompetenceDefinitionRepository
{
    private readonly IDbConnection _connection;
    private readonly IOptions<AppSettings> _appSettings;

    public SqlCompetenceDefinitionRepository(IDbConnection connection, IOptions<AppSettings> appSettings)
    {
        _connection = connection;
        _appSettings = appSettings;
    }
    /// <summary>
    /// Ottiene una lista di CompetenceDefinition che soddisfano i parametri
    /// </summary>
    /// <param name="competenceType">Tipologia di Competenza</param>
    /// <param name="processDefinitionId">ProcessDefinition per cui vale la competenza</param>
    /// <param name="tenantId">Identificativo Banca</param>
    /// <param name="participants">Lista gruppi per cui vale la competenza</param>
    /// <returns></returns>
    public Task<IEnumerable<GetCompetenceDefinitionSqlResult>> GetCompetenceDefinitionsAsync(
        string competenceType,
        string processDefinitionId,
        int tenantId,
        IReadOnlyCollection<string> participants)
    {
        if (participants == null || participants.Count == 0)
        {
            return Task.FromResult<IEnumerable<GetCompetenceDefinitionSqlResult>>(new List<GetCompetenceDefinitionSqlResult>());
        }

        using var cmd = _connection.CreateCommand();

        var isOracle = Utility.IsOracle(_appSettings.Value);
        var parameterPrefix = isOracle ? ":" : "@";

        var participantParameterNames = participants
            .Select((_, index) => $"{parameterPrefix}ParticipantId{index}")
            .ToList();

        var sql = new StringBuilder();
        sql.AppendLine("SELECT *");
        sql.AppendLine("FROM WF_CompetenceDefinition");
        sql.AppendLine($"WHERE CompetenceType = {parameterPrefix}CompetenceType");
        sql.AppendLine($"  AND ProcessDefinitionId = {parameterPrefix}ProcessDefinitionId");
        sql.AppendLine($"  AND TenantId = {parameterPrefix}TenantId");
        sql.AppendLine($"  AND ParticipantId IN ({string.Join(", ", participantParameterNames)})");

        cmd.CommandText = isOracle switch
        {
            true => sql.ToString(),
            false => sql.ToString()
        };

        AddParms(cmd, $"{parameterPrefix}CompetenceType", competenceType);
        AddParms(cmd, $"{parameterPrefix}ProcessDefinitionId", processDefinitionId);
        AddParms(cmd, $"{parameterPrefix}TenantId", tenantId);

        var participantIndex = 0;
        foreach (var participant in participants)
        {
            AddParms(cmd, $"{parameterPrefix}ParticipantId{participantIndex}", participant);
            participantIndex++;
        }

        var list = new List<GetCompetenceDefinitionSqlResult>();

        _connection.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new GetCompetenceDefinitionSqlResult
            {
                CompetenceDefinitionId = reader["CompetenceDefinitionId"].ToLongFromObject(),
                CompetenceType = reader["CompetenceType"].ToStringNullableFromObject(),
                ProcessDefinitionId = reader["ProcessDefinitionId"].ToStringNullableFromObject(),
                TenantId = reader["TenantId"].ToLongFromObject(),
                ParticipantId = reader["ParticipantId"].ToStringNullableFromObject()
            });
        }

        _connection.Close();

        return Task.FromResult<IEnumerable<GetCompetenceDefinitionSqlResult>>(list);
    }
}
