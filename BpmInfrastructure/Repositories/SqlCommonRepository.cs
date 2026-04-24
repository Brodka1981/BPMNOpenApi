using BpmInfrastructure.Common;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository.Interfaces;
using BpmInfrastructure.Results;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net.Security;
using System.Text;

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

    public Task<List<List<GetProcessInstanceSearchSqlResult>>> GetProcessInstanceSearchAsync(GetProcessInstanceSearchSqlParms parms, CancellationToken ct)
    {
        if (parms.Columns == null || parms.Columns.Count == 0)
            throw new ArgumentException("columns cannot be empty.");

        var orderedColumns = GetDistinctOrderedColumns(parms.Columns);
        var selectColumns = new List<ColumnDefinition>()
        {
            new() { Key = "processId", Expression = "pi.ProcessInstanceId", Alias = "processId" }
        };

        var joins = new List<string>();
        var filterVariableKeys = new List<string>();
        var filterVariableValues = new List<object?>();
        var filterConditions = new List<string>();
        var variableColumns = new List<string>();
        var variableIndex = 0;

        foreach (var column in orderedColumns)
        {
            if (string.Equals(column, "processId", StringComparison.OrdinalIgnoreCase))
                continue;

            // Cerca tra le colonne che non siano variabili
            if (TryGetBaseExpression(column, out var baseExpression))
            {
                selectColumns.Add(new ColumnDefinition() { Key = column, Expression = baseExpression, Alias = column });
                continue;
            }

            // Se la colonna è una variabile, fa la let join e aggiunge alla select
            var varAlias = $"v{variableIndex++}";
            joins.Add($@"LEFT JOIN WF_Variables {varAlias}
                         ON {varAlias}.ProcessInstanceId = pi.ProcessInstanceId
                        AND {varAlias}.Name = @varName_{varAlias}");
            selectColumns.Add(new ColumnDefinition() { Key = column, Expression = BuildVariableValueExpression(varAlias), Alias = column });
            variableColumns.Add(column);
        }

        // Mi crea oggetti per gestire i filtri su WF_Variables
        AppendVariableFiltersJoinsAndConditions(
            joins,
            filterConditions,
            filterVariableKeys,
            filterVariableValues,
            parms.VariableFilters);

        var cmd = _connection.CreateCommand();
        var sql = new StringBuilder();
        sql.AppendLine("SELECT");
        sql.AppendLine(string.Join(",\n", selectColumns.Select(c => $"    {c.Expression} AS [{c.Alias}]")));
        sql.AppendLine("FROM WF_ProcessDefinition pd");
        sql.AppendLine("INNER JOIN WF_ProcessInstance pi ON pd.ProcessDefinitionId = pi.ProcessDefinitionId");
        if (joins.Count > 0)
            sql.AppendLine(string.Join("\n", joins));
        AppendFilters(sql, cmd, parms.Categories, parms.IdStates, parms.DefinitionTypes, filterConditions);
        sql.AppendLine("ORDER BY pi.ProcessInstanceId");

        cmd.CommandText = sql.ToString();

        for (var i = 0; i < variableColumns.Count; i++)
            AddParms(cmd, $"@varName_v{i}", variableColumns[i]);

        for (var i = 0; i < filterVariableKeys.Count; i++)
        {
            AddParms(cmd, $"@filterVarName_f{i}", filterVariableKeys[i]);
            AddParms(cmd, $"@filterVarValue_f{i}", filterVariableValues[i]);
        }


        var result = ExecuteSearchCommand(cmd, selectColumns.Select(x => x.Key).ToList());
        return Task.FromResult(result);
    }

    public Task<List<List<GetProcessInstanceSearchSqlResult>>> GetProcessInstanceSearchAndGroupByAsync(GetProcessInstanceSearchSqlParms parms, CancellationToken ct)
    {
        if (parms.Columns == null || parms.Columns.Count == 0)
            throw new ArgumentException("columns cannot be empty.");

        var orderedColumns = GetDistinctOrderedColumns(parms.Columns)
            .Where(x => !string.Equals(x, "processId", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var groupingColumns = new List<ColumnDefinition>();
        var joins = new List<string>();
        var filterVariableKeys = new List<string>();
        var filterVariableValues = new List<object?>();
        var filterConditions = new List<string>();
        var variableColumns = new List<string>();
        var variableIndex = 0;

        foreach (var column in orderedColumns)
        {
            // cerca di capire se la colonna non è una variabile
            if (TryGetBaseExpression(column, out var baseExpression))
            {
                groupingColumns.Add(new ColumnDefinition() { Key = column, Expression = baseExpression, Alias = column });
                continue;
            }

            var varAlias = $"v{variableIndex++}";
            joins.Add($@"LEFT JOIN WF_Variables {varAlias}
                         ON {varAlias}.ProcessInstanceId = pi.ProcessInstanceId
                        AND {varAlias}.Name = @varName_{varAlias}");
            groupingColumns.Add(new ColumnDefinition() { Key = column, Expression = BuildVariableValueExpression(varAlias), Alias = column });
            variableColumns.Add(column);
        }

        AppendVariableFiltersJoinsAndConditions(
            joins,
            filterConditions,
            filterVariableKeys,
            filterVariableValues,
            parms.VariableFilters);

        var cmd = _connection.CreateCommand();
        var sql = new StringBuilder();
        sql.AppendLine("SELECT");
        sql.AppendLine("    COUNT(DISTINCT pi.ProcessInstanceId) AS [count]");
        if (groupingColumns.Count > 0)
            sql.AppendLine($",\n{string.Join(",\n", groupingColumns.Select(c => $"    {c.Expression} AS [{c.Alias}]"))}");
        sql.AppendLine("FROM WF_ProcessDefinition pd");
        sql.AppendLine("INNER JOIN WF_ProcessInstance pi ON pd.ProcessDefinitionId = pi.ProcessDefinitionId");
        if (joins.Count > 0)
            sql.AppendLine(string.Join("\n", joins));
        AppendFilters(sql, cmd, parms.Categories, parms.IdStates, parms.DefinitionTypes, filterConditions);
        if (groupingColumns.Count > 0)
            sql.AppendLine($"GROUP BY {string.Join(", ", groupingColumns.Select(c => c.Expression))}");

        cmd.CommandText = sql.ToString();

        for (var i = 0; i < variableColumns.Count; i++)
            AddParms(cmd, $"@varName_v{i}", variableColumns[i]);

        for (var i = 0; i < filterVariableKeys.Count; i++)
        {
            AddParms(cmd, $"@filterVarName_f{i}", filterVariableKeys[i]);
            AddParms(cmd, $"@filterVarValue_f{i}", filterVariableValues[i]);
        }

        var keys = new List<string>() { "count" };
        keys.AddRange(groupingColumns.Select(x => x.Key));
        var result = ExecuteSearchCommand(cmd, keys);
        return Task.FromResult(result);
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

    #region Metodi per SearchProcess

    private static List<string> GetDistinctOrderedColumns(List<string> columns)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var output = new List<string>();

        foreach (var column in columns)
        {
            if (string.IsNullOrWhiteSpace(column))
                continue;

            if (set.Add(column))
                output.Add(column);
        }

        return output;
    }
    // Mappa le colonne che non sono variabili
    private static bool TryGetBaseExpression(string column, out string expression)
    {
        switch (column)
        {
            case "processId":
                expression = "pi.ProcessInstanceId";
                return true;
            case "category":
                expression = "pd.Category";
                return true;
            case "idState":
                expression = "pi.CurrentNodeId";
                return true;
            case "definitionType":
                expression = "pd.Type";
                return true;
            default:
                expression = string.Empty;
                return false;
        }
    }
    // Trova il giusto valore per una variabile
    private static string BuildVariableValueExpression(string alias)
    {
        return $@"CASE
                    WHEN {alias}.ValueString IS NOT NULL THEN CAST(CAST({alias}.ValueString AS NVARCHAR(4000)) AS sql_variant)
                    WHEN {alias}.ValueNumber IS NOT NULL THEN CAST({alias}.ValueNumber AS sql_variant)
                    WHEN {alias}.ValueDate IS NOT NULL THEN CAST({alias}.ValueDate AS sql_variant)
                    WHEN {alias}.ValueBoolean IS NOT NULL THEN CAST({alias}.ValueBoolean AS sql_variant)
                    WHEN {alias}.ValueJson IS NOT NULL THEN CAST(CAST({alias}.ValueJson AS NVARCHAR(4000)) AS sql_variant)
                    ELSE NULL
                  END";
    }


    private string AddInParameters(IDbCommand cmd, string prefix, List<string> values)
    {
        var names = new List<string>();
        for (var i = 0; i < values.Count; i++)
        {
            var parameterName = $"@{prefix}{i}";
            names.Add(parameterName);
            AddParms(cmd, parameterName, values[i]);
        }

        return string.Join(", ", names);
    }

    // Metodo per aggiungere i filtri
    private void AppendFilters(StringBuilder sql,IDbCommand cmd,List<string>? categories,List<string>? idStates,List<string>? definitionTypes, List<string>? variableFilters)
    {
        var filters = new List<string>();

        if (categories != null && categories.Count > 0)
            filters.Add($"pd.Category IN ({AddInParameters(cmd, "category", categories)})");

        if (idStates != null && idStates.Count > 0)
            filters.Add($"pi.CurrentNodeId IN ({AddInParameters(cmd, "idState", idStates)})");

        if (definitionTypes != null && definitionTypes.Count > 0)
            filters.Add($"pd.Type IN ({AddInParameters(cmd, "definitionType", definitionTypes)})");

        if (variableFilters != null && variableFilters.Count > 0)
            filters.AddRange(variableFilters);

        if (filters.Count == 0)
            return;

        sql.AppendLine($"WHERE {string.Join(" AND ", filters)}");
    }

    private void AppendVariableFiltersJoinsAndConditions(
        List<string> joins,
        List<string> filterConditions,
        List<string> filterVariableKeys,
        List<object?> filterVariableValues,
        List<VariableFilterItemDto>? variableFilters)
    {
        if (variableFilters == null || variableFilters.Count == 0)
            return;

        var filterIndex = 0;
        foreach (var variableFilter in variableFilters)
        {
            if (string.IsNullOrWhiteSpace(variableFilter.Key))
                continue;

            var filterAlias = $"f{filterIndex}";
            joins.Add($@"INNER JOIN WF_Variables {filterAlias}
                         ON {filterAlias}.ProcessInstanceId = pi.ProcessInstanceId
                        AND {filterAlias}.Name = @filterVarName_{filterAlias}");

            var variableExpression = BuildVariableValueExpression(filterAlias);
            var parameterName = $"@filterVarValue_{filterAlias}";
            var sqlCondition = (variableFilter.Condition ?? "EQ").ToUpperInvariant() switch
            {
                "EQ" => $"{variableExpression} = {parameterName}",
                "LT" => $"{variableExpression} < {parameterName}",
                "GT" => $"{variableExpression} > {parameterName}",
                "LIKE" => $"CAST({variableExpression} AS NVARCHAR(4000)) LIKE {parameterName}",
                _ => throw new ArgumentException($"Unsupported condition '{variableFilter.Condition}'.")
            };

            filterConditions.Add(sqlCondition);
            filterVariableKeys.Add(variableFilter.Key);
            filterVariableValues.Add(variableFilter.Value);
            filterIndex++;
        }
    }


    private List<List<GetProcessInstanceSearchSqlResult>> ExecuteSearchCommand(IDbCommand cmd, List<string> keys)
    {
        var rows = new List<List<GetProcessInstanceSearchSqlResult>>();
        _connection.Open();
        try
        {
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new List<GetProcessInstanceSearchSqlResult>();
                foreach (var key in keys)
                {
                    var value = reader[key];
                    row.Add(new GetProcessInstanceSearchSqlResult
                    {
                        Key = key,
                        Value = value == DBNull.Value ? null : value
                    });
                }

                rows.Add(row);
            }
        }
        finally
        {
            _connection.Close();
        }

        return rows;
    }

    private sealed class ColumnDefinition
    {
        public string Key { get; set; } = string.Empty;
        public string Expression { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
    }

    #endregion

}