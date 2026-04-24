using BpmInfrastructure.Common;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository;
using BpmInfrastructure.Repository.Interfaces;
using Microsoft.Extensions.Options;
using System.Data;

namespace BpmInfrastructure.Repositories;

public class SqlDefinitionRepository(IDbConnection connection, IOptions<AppSettings> appSettings) : SqlRepository, IDefinitionRepository
{
    private readonly IDbConnection _connection = connection;
    private readonly IOptions<AppSettings> _appSettings = appSettings;

    public Task<string> GetDefinitionXmlAsync(long definitionId, string company, CancellationToken ct)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            SELECT BpmnXml
            FROM WF_ProcessDefinition
            WHERE ProcessDefinitionId = @ProcessDefinitionId
        ";

        //WHERE ProcessDefinitionId = @DefinitionId AND Company = @Company

        AddParms(cmd, "@ProcessDefinitionId", definitionId);
        //AddParms(cmd, "@Company", company);

        _connection.Open();
        var result = cmd.ExecuteScalar();
        _connection.Close();

        return Task.FromResult(result?.ToString() ?? "");
    }

    public Task<IEnumerable<WorkflowDefinitionInfo>> GetDefinitionsAsync(string? category, CancellationToken ct)
    {
        using var cmd = _connection.CreateCommand();

        //run sql commands for the correct type
        cmd.CommandText = Utility.IsOracle(_appSettings.Value) switch
        {
            true => @"
                        SELECT  * 
                        FROM WF_ProcessDefinition
                        WHERE (@Category IS NULL OR [Category] = @Category)
                        ORDER BY [Name]
                    ",
            false => @"
                        SELECT  * 
                        FROM WF_ProcessDefinition
                        WHERE (@Category IS NULL OR [Category] = @Category)
                        ORDER BY [Name]
                    "
        };

        AddParms(cmd, "@Category", string.IsNullOrWhiteSpace(category) ? DBNull.Value : category!);

        var list = new List<WorkflowDefinitionInfo>();

        _connection.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new WorkflowDefinitionInfo
            {
                ProcessType = reader["Type"]?.ToString() ?? "",
                Category = reader["Category"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? ""
            });
        }
        _connection.Close();

        return Task.FromResult<IEnumerable<WorkflowDefinitionInfo>>(list);
    }
}