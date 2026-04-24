using BpmInfrastructure.Models;
using BpmInfrastructure.Common;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository.Interfaces;
using Microsoft.Extensions.Options;
using System.Data;

namespace BpmInfrastructure.Repository;

public class SqlProcessInstanceRepository : SqlRepository, IProcessInstanceRepository
{
    private readonly IDbConnection _connection;
    private readonly IOptions<AppSettings> _appSettings;

    public SqlProcessInstanceRepository(IDbConnection connection, IOptions<AppSettings> appSettings)
    {
        _connection = connection;
        _appSettings = appSettings;
    }
    /// <summary>
    /// Salva una nuova Istanza di Processo a DB con i valori di Start
    /// </summary>
    /// <param name="instance">Processo da creare</param>
    /// <param name="ct"></param>
    /// <returns>Id Istanza di Processo creata</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Task<long> SaveAsync(ProcessInstance instance, CancellationToken ct)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = Utility.IsOracle(_appSettings.Value) switch
        {
            true => @"
                INSERT INTO WF_ProcessInstance
                (
                    ProcessDefinitionId,
                    Status,
                    StartedAt,
                    CurrentNodeId,
                    LastUpdatedAt,
                    TenantId
                )
                VALUES
                (
                    @ProcessDefinitionId,
                    @Status,
                    CURRENT_TIMESTAMP,
                    @CurrentNodeId,
                    CURRENT_TIMESTAMP,
                    @TenantId
                )
            ",
            false => @"
                INSERT INTO WF_ProcessInstance
                (
                    ProcessDefinitionId,
                    Status,
                    StartedAt,
                    CurrentNodeId,
                    LastUpdatedAt,
                    TenantId
                )
                VALUES
                (
                    @ProcessDefinitionId,
                    @Status,
                    GETDATE(),
                    @CurrentNodeId,
                    GETDATE(),
                    @TenantId
                );
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
                "
        };

        AddParms(cmd, "@ProcessDefinitionId", instance.ProcessDefinitionId);
        AddParms(cmd, "@Status", instance.Status);
        AddParms(cmd, "@CurrentNodeId", instance.CurrentNodeId);
        AddParms(cmd, "@TenantId", instance.TenantId);

        _connection.Open();
        try
        {
            long processInstanceId;
            if (Utility.IsOracle(_appSettings.Value))
            {
                cmd.ExecuteNonQuery();

                using var getIdCmd = _connection.CreateCommand();
                getIdCmd.CommandText = "SELECT MAX(ProcessInstanceId) FROM WF_ProcessInstance WHERE TenantId = @TenantId";
                AddParms(getIdCmd, "@TenantId", instance.TenantId);
                processInstanceId = Convert.ToInt64(getIdCmd.ExecuteScalar());
            }
            else
            {
                var scalarResult = cmd.ExecuteScalar();
                if (scalarResult == null || scalarResult == DBNull.Value)
                    throw new InvalidOperationException("Insert su WF_ProcessInstance completato ma ProcessInstanceId non restituito.");

                processInstanceId = Convert.ToInt64(scalarResult);
            }

            return Task.FromResult(processInstanceId);
        }
        finally
        {
            _connection.Close();
        }
    }
}