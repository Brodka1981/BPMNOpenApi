using BpmInfrastructure.Common;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository.Interfaces;
using BpmInfrastructure.Results;
using Microsoft.Extensions.Options;
using System.Data;

namespace BpmInfrastructure.Repository;

public class SqlProcessDefinitionRepository : SqlRepository, IProcessDefinitionRepository
{
    private readonly IDbConnection _connection;
    private readonly IOptions<AppSettings> _appSettings;

    public SqlProcessDefinitionRepository(IDbConnection connection, IOptions<AppSettings> appSettings)
    {
        _connection = connection;
        _appSettings = appSettings;
    }
    /// <summary>
    /// Ottiene tutte le Definizioni dei Processi, filtrati per categoria se presente
    /// </summary>
    /// <param name="category"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<IEnumerable<WorkflowDefinitionInfo>> GetProcessDefinitionsAsync(string? category, CancellationToken ct)
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

    public Task<GetProcessDefinitionSqlResult?> GetProcessDefinitionByTypeAsync(string type, int company, CancellationToken ct)
    {
        using var cmd = _connection.CreateCommand();

        cmd.CommandText = Utility.IsOracle(_appSettings.Value) switch
        {
            true => @"
                        SELECT *
                        FROM (
                            SELECT *
                            FROM WF_ProcessDefinition
                            WHERE Type = @Type
                              AND IsActive = 1
                              AND TenantId = @Company
                            ORDER BY ProcessDefinitionId DESC
                        )
                        WHERE ROWNUM = 1
                    ",
            false => @"
                        SELECT TOP 1 *
                        FROM WF_ProcessDefinition
                        WHERE Type = @Type
                          AND IsActive = 1
                          AND TenantId = @Company
                        ORDER BY ProcessDefinitionId DESC
                    "
        };

        AddParms(cmd, "@Type", type);
        AddParms(cmd, "@Company", company);

        GetProcessDefinitionSqlResult? data = null;

        _connection.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            data = new GetProcessDefinitionSqlResult
            {
                ProcessDefinitionId = reader["ProcessDefinitionId"].ToLongFromObject(),
                Type = reader["Type"].ToStringFromObject(),
                Category = reader["Category"].ToStringFromObject(),
                Name = reader["Name"].ToStringFromObject(),
                BpmnXml = reader["BpmnXml"].ToStringFromObject(),
                IsActive = reader["IsActive"].ToBoolFromObject(),
                CreatedAt = reader["CreatedAt"].ToDateTimeFromObject(),
                CreatedBy = reader["CreatedBy"].ToStringFromObject(),
                TenantId = reader["TenantId"].ToLongFromObject()
            };
        }
        _connection.Close();

        return Task.FromResult(data);
    }

}