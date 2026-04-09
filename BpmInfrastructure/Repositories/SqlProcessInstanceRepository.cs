using BpmInfrastructure.Models;
using BpmInfrastructure.Repository.Interfaces;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text.Json;

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

    public Task<long> GenerateProcessIdAsync(CancellationToken ct)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT NEXT VALUE FOR Seq_ProcessInstanceId;";

        _connection.Open();
        var id = (long)cmd.ExecuteScalar();
        _connection.Close();

        return Task.FromResult(id);
    }

    public Task SaveAsync(ProcessInstance instance, CancellationToken ct)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ProcessInstances
            (ProcessId, DefinitionId, Company, [User], VariablesJson, CurrentNode, CreatedAt)
            VALUES
            (@Id, @DefinitionId, @Company, @User, @VariablesJson, @CurrentNode, GETUTCDATE())
        ";

        AddParms(cmd, "@Id", instance.Id);
        AddParms(cmd, "@DefinitionId", instance.DefinitionId);
        AddParms(cmd, "@Company", instance.Company);
        AddParms(cmd, "@User", instance.User);
        AddParms(cmd, "@VariablesJson", JsonSerializer.Serialize(instance.Variables));
        AddParms(cmd, "@CurrentNode", instance.CurrentNode);

        _connection.Open();
        cmd.ExecuteNonQuery();
        _connection.Close();

        return Task.CompletedTask;
    }
}