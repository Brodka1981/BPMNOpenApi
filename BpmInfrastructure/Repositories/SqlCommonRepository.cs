using BpmInfrastructure.Common;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository.Interfaces;
using BpmInfrastructure.Results;
using Microsoft.Extensions.Options;
using System.Data;

namespace BpmInfrastructure.Repository;

public class SqlCommonRepository : SqlRepository, ISqlCommonRepository
{
    private readonly IDbConnection _connection;
    private readonly IOptions<AppSettings> _appSettings;

    public SqlCommonRepository(IDbConnection connection, IOptions<AppSettings> appSettings)
    {
        _connection = connection;
        _appSettings = appSettings;
    }

    /// <summary>
    /// Get Process Definition Async
    /// </summary>
    /// <param name="parms"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<GetProcessDefinitionSqlResult?> GetProcessDefinitionAsync(GetProcessDefinitionSqlParms parms, CancellationToken ct)
    {
        GetProcessDefinitionSqlResult? data = null;
        using var cmd = _connection.CreateCommand();

        //run sql commands for the correct type
        cmd.CommandText = Utility.IsOracle(_appSettings.Value) switch
        {
            true => @"
                    SELECT * FROM WF_ProcessDefinition WHERE ProcessDefinitionId = @ProcessDefinitionId
                ",
            false => @"
                    SELECT * FROM WF_ProcessDefinition WHERE ProcessDefinitionId = @ProcessDefinitionId
                "
        };

        AddParms(cmd, "@ProcessDefinitionId", parms.ProcessDefinitionId);

        _connection.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            data = new GetProcessDefinitionSqlResult()
            {
                ProcessDefinitionId = reader["ProcessDefinitionId"].ToLongFromObject(),
                Type = reader["Type"].ToStringFromObject(),
                Name = reader["Type"].ToStringFromObject(),
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

    /// <summary>
    /// Get Process Instance Async
    /// </summary>
    /// <param name="parms"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<GetProcessInstanceSqlResult?> GetProcessInstanceAsync(GetProcessInstanceSqlParms parms, CancellationToken ct)
    {
        GetProcessInstanceSqlResult? data = null;
        using var cmd = _connection.CreateCommand();

        //run sql commands for the correct type
        cmd.CommandText = Utility.IsOracle(_appSettings.Value) switch
        {
            true => @"
                    SELECT * FROM WF_ProcessInstance WHERE ProcessInstanceId = @ProcessInstanceId
                ",
            false => @"
                    SELECT * FROM WF_ProcessInstance WHERE ProcessInstanceId = @ProcessInstanceId
                "
        };

        AddParms(cmd, "@ProcessInstanceId", parms.ProcessInstanceId);

        _connection.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            data = new GetProcessInstanceSqlResult()
            {
                ProcessInstanceId = reader["ProcessInstanceId"].ToLongFromObject(),
                ProcessDefinitionId = reader["ProcessDefinitionId"].ToLongFromObject(),
                Status = reader["Status"].ToStringFromObject(),
                StartedAt = reader["StartedAt"].ToDateTimeFromObject(),
                CurrentNodeId = reader["CurrentNodeId"].ToStringFromObject(),
                CurrentUserTaskId = reader["CurrentUserTaskId"].ToStringFromObject(),
                CompletedAt =  reader["CompletedAt"].ToDateTimeNullableFromObject(),
                LastUpdatedAt = reader["LastUpdatedAt"].ToDateTimeFromObject(),
                TenantId = reader["TenantId"].ToLongFromObject()
            };
        }

        _connection.Close();

        return Task.FromResult(data);
    }

    /// <summary>
    /// Get Variables Async
    /// </summary>
    /// <param name="parms"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<GetVariableSqlResult?> GetVariablesAsync(GetVariableSqlParms parms, CancellationToken ct)
    {
        GetVariableSqlResult? data = null;
        using var cmd = _connection.CreateCommand();

        //run sql commands for the correct type
        cmd.CommandText = Utility.IsOracle(_appSettings.Value) switch
        {
            true => @"
                    SELECT * FROM WF_Variables WHERE ProcessInstanceId = @ProcessInstanceId
                ",
            false => @"
                    SELECT * FROM WF_Variables WHERE ProcessInstanceId = @ProcessInstanceId
                "
        };

        AddParms(cmd, "@ProcessInstanceId", parms.ProcessInstanceId);

        _connection.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (data == null) 
                data = new GetVariableSqlResult() { List = new List<GetVariableSqlValues>() { } };

            data?.List?.Add( new GetVariableSqlValues()
            {
                VariableId = reader["VariableId"].ToLongFromObject(),
                TenantId = reader["TenantId"].ToLongFromObject(),
                ProcessInstanceId = reader["ProcessInstanceId"].ToLongFromObject(),
                Type = reader["Type"].ToStringFromObject(),
                Name = reader["Name"].ToStringFromObject(),
                ValueType = reader["ValueType"].ToStringFromObject(),
                ValueString = reader["ValueString"].ToStringFromObject(),
                ValueNumber = reader["ValueNumber"].ToDecimalNullableFromObject(),
                ValueDate = reader["ValueDate"].ToDateTimeNullableFromObject(),
                ValueBoolean = reader["ValueBoolean"].ToBoolNullableFromObject(),
                ValueJson = reader["ValueJson"].ToStringFromObject()               
            });
        }

        _connection.Close();

        return Task.FromResult(data);
    }

    /// <summary>
    /// Is User Activity Complete Async
    /// </summary>
    /// <param name="parms"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<bool> IsUserActivityCompleteAsync(IsUserActivityCompleteSqlParms parms, CancellationToken ct)
    {
        var isUserActivityComplete = false;
        using var cmd = _connection.CreateCommand();

        //run sql commands for the correct type
        cmd.CommandText = Utility.IsOracle(_appSettings.Value) switch
        {
            true => @"
                    SELECT count(*) AS CountTasks FROM WF_UserTasks WHERE [name] = @StateName AND ProcessInstanceId = @ProcessInstanceId AND CompletedAt IS NOT NULL
                ",
            false => @"
                    SELECT count(*) AS CountTasks FROM WF_UserTasks WHERE [name] = @StateName AND ProcessInstanceId = @ProcessInstanceId AND CompletedAt IS NOT NULL
                "
        };

        AddParms(cmd, "@StateName", parms.StateName.ToStringFromObject());
        AddParms(cmd, "@ProcessInstanceId", parms.ProcessInstanceId);

        _connection.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (reader["CountTasks"].ToLongFromObject() > 0)
                isUserActivityComplete = true;
        }

        _connection.Close();

        return Task.FromResult(isUserActivityComplete);
    }
}