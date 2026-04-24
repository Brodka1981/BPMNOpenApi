using BpmDomain.Commands;
using BpmDomain.Common;
using BpmDomain.Engine.Interfaces;
using BpmDomain.Factories.Interfaces;
using BpmDomain.Models;
using BpmDomain.Results;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository.Interfaces;
using System.Text.Json;

namespace BpmDomain.Engine;

public class BpmEngine(
    IProcessInstanceRepository instanceRepo,
    IProcessDefinitionRepository processDefinitionRepo,
    ISqlCommonRepository contextRepo,
    IBpmnParserService parser,
    IAuthorizationService auth,
    IServiceFactory serviceFactory
    ) : IBpmEngine
{
    private readonly IProcessDefinitionRepository _processDefinitionRepo = processDefinitionRepo;
    private readonly IProcessInstanceRepository _instanceRepo = instanceRepo;
    private readonly ISqlCommonRepository _contextRepo = contextRepo;
    private readonly IServiceFactory _serviceFactory = serviceFactory;
    private readonly IBpmnParserService _parser = parser;
    private readonly IAuthorizationService _auth = auth;

    /// <summary>
    /// Start Process Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<StartProcessResult> StartProcessAsync(StartProcessCommand cmd, CancellationToken ct = default)
    {
        if (!int.TryParse(cmd.Company, out var cmpny))
            throw new ArgumentException("Company not valid");

        // Ottiene la definizione del Processo da creare
        var processDefinition = await _processDefinitionRepo.GetProcessDefinitionByTypeAsync(cmd.ProcessType, cmpny, ct);

        if (processDefinition == null)
            throw new KeyNotFoundException($"Process definition '{cmd.ProcessType}' not found");

        // Controlla che l'utente abbia i diritti per startare il processo
        if (!await _auth.CanStartAsync(processDefinition.ProcessDefinitionId.ToString(), cmd.User, cmd.Company, ct))
            throw new UnauthorizedAccessException("User cannot start this process");

        var workflowDefinition = _parser.Parse(FixXmlFormat(processDefinition.BpmnXml));

        // Esegue inserimento della nuova istanza di Processo

        var instance = new ProcessInstance
        {
            ProcessDefinitionId = processDefinition.ProcessDefinitionId,
            Status = "start",
            CurrentNodeId = workflowDefinition?.StartEventId,
            TenantId = cmpny,
            User = cmd.User,
            Variables = cmd.Variables
        };

        var newId = await _instanceRepo.SaveAsync(instance, ct);

        return new StartProcessResult() { ProcessId = newId };
    }

    /// <summary>
    /// Get Definitions Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IEnumerable<GetDefinitionsResult>> GetDefinitionsAsync(GetDefinitionsCommand cmd, CancellationToken ct = default)
    {
        /***** TODO: CONTROLLI AUTORIZZAZIONI
        if (!await _auth.CanGetDefinitionsAsync("TODO", cmd.User, cmd.Company, ct))
            throw new UnauthorizedAccessException("User cannot start this process");
        */

        var definitions = await _processDefinitionRepo.GetProcessDefinitionsAsync(cmd.Category, ct);

        return definitions.Select(x => new GetDefinitionsResult()
        {
            ProcessType = x.ProcessType,
            Name = x.Name,
            Category = x.Category
        });
    }

    /// <summary>
    /// Get Context Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Results.GetContextResult> GetContextAsync(GetContextCommand cmd, CancellationToken ct = default)
    {
        //if (!await _auth.CanGetContextAsync("TODO", cmd.User, cmd.Company, ct))
        //    throw new UnauthorizedAccessException("User cannot start this process");

        var result = new GetContextResult() { };

        var processInstance = await _contextRepo.GetProcessInstanceAsync(
            new GetProcessInstanceSqlParms()
            {
                ProcessInstanceId = cmd.ProcessInstanceId, 
                User = cmd.User, 
                Company = cmd.Company 
            }, ct);

        if (processInstance != null)
        {
            var processDefinition = await _contextRepo.GetProcessDefinitionAsync(
                new GetProcessDefinitionSqlParms()
                {
                    ProcessDefinitionId = processInstance.ProcessDefinitionId,
                    User = cmd.User,
                    Company = cmd.Company
                }, ct);

            var getVariableSqlResult = await _contextRepo.GetVariablesAsync(
                new GetVariableSqlParms()
                {
                    ProcessInstanceId = processInstance.ProcessInstanceId,
                    User = cmd.User,
                    Company = cmd.Company
                }, ct);

            var isUserActivityComplete = await _contextRepo.IsUserActivityCompleteAsync(
                new IsUserActivityCompleteSqlParms()
                {
                    ProcessInstanceId = processInstance.ProcessInstanceId,
                    StateName = processInstance.CurrentNodeId,
                    User = cmd.User,
                    Company = cmd.Company
                }, ct);

            if (processDefinition != null && getVariableSqlResult?.List != null)
            {
                var workflowDefinition = _parser.Parse(FixXmlFormat(processDefinition.BpmnXml));

                var toGetContextParms = new ToGetContextParms()
                {
                    Variables = getVariableSqlResult.List,
                    CurrentState = processInstance.CurrentNodeId,
                    ProcessInstanceId = cmd.ProcessInstanceId,
                    Name = processDefinition.Name,
                    ProcessType = processDefinition.Type,
                    ContextMode = isUserActivityComplete ? "MODIFY" : "NORMAL",
                    CurrentHttpContext = cmd.CurrentHttpContext,
                    AppSettings = new AppSettings() { },
                    Abi = cmd.Abi
                };

                result = workflowDefinition.ToGetContextResult(toGetContextParms, _serviceFactory);
            }
        }

        return result;
    }

    private static string FixXmlFormat(string? xmlString)
    {
        xmlString ??= string.Empty;
        //correct any incorrect xml format
        xmlString = xmlString.Replace("\r\n", "");

        return xmlString;
    }

    public async Task<SearchProcessResult> SearchProcessAsync(SearchProcessCommand query, CancellationToken ct = default)
    {
        /***** TODO: CONTROLLI AUTORIZZAZIONI
        if (!await _auth.CanSearchProcessAsync("TODO", cmd.User, cmd.Company, ct))
            throw new UnauthorizedAccessException("User cannot start this process");
        /***** TODO: FLAG isClosed
        /***** TODO: Ordinamento
        /***** TODO: Paginazione
        /***** TODO: exportType (CSV/JSON)
        */

        /*var variablesFilters = new List<VariableFilterItemDto>
        {
            new VariableFilterItemDto { Key = "variable1", Condition = "LT", Value = 2.0000000000m }
        };*/
        var variableFilters = BuildVariableFilters(query.Filters);
        var parms = new GetProcessInstanceSearchSqlParms()
        {
            Categories = query.Category?.ToList() ?? new List<string>(),
            IdStates = query.IdState?.ToList() ?? new List<string>(),
            DefinitionTypes = query.DefinitionType?.ToList() ?? new List<string>(),
            VariableFilters = variableFilters,
            Columns = query.Columns?.ToList() ?? new List<string>()
        };

        // Caso SEARCH Normale
        if (query.Grouped != true)
        {
            var processInstanceSearch = await _contextRepo
                .GetProcessInstanceSearchAsync(parms, ct);

            // TODO: mappatura risultati
            return new SearchProcessResult

            {
                Items = processInstanceSearch,
                Page = 1,
                Size = 10,
                TotalElements = 1,
                TotalPages = 1
            };
        }

        // METODO SEARCH GROUPBY
        var processInstanceGroupBy = await _contextRepo.GetProcessInstanceSearchAndGroupByAsync(
            /*new GetProcessInstanceSearchSqlParms()
            {
                Categories = ["POC"],
                Columns = ["category", "warning1"]
            }*/parms, ct);

        return new SearchProcessResult
        {
            Items = processInstanceGroupBy,
            Page = 1,
            Size = 10,
            TotalElements = 1,
            TotalPages = 1
        };

    }

    private static List<VariableFilterItemDto> BuildVariableFilters(object[]? filters)
    {
        if (filters == null || filters.Length == 0)
            return [];

        var result = new List<VariableFilterItemDto>();

        foreach (var filter in filters)
        {
            if (filter is VariableFilterItemDto typedFilter)
            {
                result.Add(typedFilter);
                continue;
            }

            if (filter is JsonElement element && element.ValueKind == JsonValueKind.Object)
            {
                var item = new VariableFilterItemDto
                {
                    Key = GetStringProperty(element, "key"),
                    Condition = GetStringProperty(element, "condition"),
                    Value = GetObjectProperty(element, "value")
                };

                if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Condition))
                    result.Add(item);
            }
        }

        return result;
    }

    private static string GetStringProperty(JsonElement element, string propertyName)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (!property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                continue;

            return property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString() ?? string.Empty
                : property.Value.ToString();
        }

        return string.Empty;
    }

    private static object? GetObjectProperty(JsonElement element, string propertyName)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (!property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                continue;

            return property.Value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number when property.Value.TryGetInt64(out var intValue) => intValue,
                JsonValueKind.Number when property.Value.TryGetDecimal(out var decimalValue) => decimalValue,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => property.Value.ToString()
            };
        }

        return null;
    }

}