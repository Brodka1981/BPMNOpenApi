using BpmDomain.Commands;
using BpmDomain.Common;
using BpmDomain.Engine.Interfaces;
using BpmDomain.Models;
using BpmDomain.Results;
using BpmInfrastructure.Models;

namespace BpmDomain.Engine;

public class BpmEngine : IBpmEngine
{
    private readonly BpmInfrastructure.Repository.Interfaces.IProcessDefinitionRepository _definitionRepo;
    private readonly BpmInfrastructure.Repository.Interfaces.IProcessInstanceRepository _instanceRepo;
    private readonly BpmInfrastructure.Repository.Interfaces.ISqlCommonRepository _contextRepo;

    private readonly IBpmnParserService _parser;
    private readonly IAuthorizationService _auth;

    public BpmEngine(
        BpmInfrastructure.Repository.Interfaces.IProcessDefinitionRepository definitionRepo,
        BpmInfrastructure.Repository.Interfaces.IProcessInstanceRepository instanceRepo,
        BpmInfrastructure.Repository.Interfaces.ISqlCommonRepository contextRepo,
        IBpmnParserService parser,
        IAuthorizationService auth)
    {
        _definitionRepo = definitionRepo;
        _instanceRepo = instanceRepo;
        _contextRepo = contextRepo;
        _parser = parser;
        _auth = auth;
    }

    /// <summary>
    /// Start Process Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<EngineStartResult> StartProcessAsync(EngineStartCommand cmd, CancellationToken ct = default)
    {
        if (!int.TryParse(cmd.Company, out var cmpny))
            throw new ArgumentException("Company not valid");

        // Ottiene la definizione del Processo da creare
        var processDefinition = await _definitionRepo.GetProcessDefinitionByTypeAsync(cmd.DefinitionId, cmpny, ct);

        if (processDefinition == null)
            throw new KeyNotFoundException($"Process definition '{cmd.DefinitionId}' not found");

        // Controlla che l'utente abbia i diritti per startare il processo
        if (!await _auth.CanStartAsync(processDefinition.ProcessDefinitionId.ToString(), cmd.User, cmd.Company, ct))
            throw new UnauthorizedAccessException("User cannot start this process");

        var workflowDefinition = _parser.Parse(FixXmlFormat(processDefinition.BpmnXml));


        // TODO: Evitare questa riga, farsi direttamente restituire l'id del Processo creato dalla _instanceRepo.SaveAsync(instance, ct);
        long newId = await _instanceRepo.GenerateProcessIdAsync(ct);

        var instance = new ProcessInstance
        {
            Id = newId,
            DefinitionId = cmd.DefinitionId,
            Company = cmd.Company,
            User = cmd.User,
            Variables = cmd.Variables,
            CurrentNode = workflowDefinition.StartEventId
        };

        await _instanceRepo.SaveAsync(instance, ct);

        return new EngineStartResult(newId);
    }

    /// <summary>
    /// Get Definitions Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IEnumerable<EngineDefinitionResult>> GetDefinitionsAsync(EngineGetDefinitionsCommand cmd, CancellationToken ct = default)
    {
        var definitions = await _definitionRepo.GetProcessDefinitionsAsync(cmd.Category, ct);

        return definitions.Select(x => new EngineDefinitionResult(
            x.ProcessType,
            x.Name,
            x.Category
        ));
    }

    /// <summary>
    /// Get Context Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Results.GetContextResult> GetContextAsync(GetContextCommand cmd, CancellationToken ct = default)
    {
        var result = new GetContextResult() { };

        var processInstance = await _contextRepo.GetProcessInstanceAsync(
            new BpmInfrastructure.Models.GetProcessInstanceSqlParms()
            {
                ProcessInstanceId = cmd.ProcessInstanceId, 
                User = cmd.User, 
                Company = cmd.Company 
            }, ct);

        if (processInstance != null)
        {
            var processDefinition = await _contextRepo.GetProcessDefinitionAsync(
                new BpmInfrastructure.Models.GetProcessDefinitionSqlParms()
                {
                    ProcessDefinitionId = processInstance.ProcessDefinitionId,
                    User = cmd.User,
                    Company = cmd.Company
                }, ct);

            var getVariableSqlResult = await _contextRepo.GetVariablesAsync(
                new BpmInfrastructure.Models.GetVariableSqlParms()
                {
                    ProcessInstanceId = processInstance.ProcessInstanceId,
                    User = cmd.User,
                    Company = cmd.Company
                }, ct);

            var isUserActivityComplete = await _contextRepo.IsUserActivityCompleteAsync(
                new BpmInfrastructure.Models.IsUserActivityCompleteSqlParms()
                {
                    ProcessInstanceId = processInstance.ProcessInstanceId,
                    StateName = processInstance.CurrentNodeId,
                    User = cmd.User,
                    Company = cmd.Company
                }, ct);

            if (processDefinition != null && getVariableSqlResult?.List != null)
            {
                var workflowDefinition = _parser.Parse(FixXmlFormat(processDefinition.BpmnXml));

                var toGetContextParms = new ToGetContextParms() {
                    Variables = getVariableSqlResult.List, 
                    CurrentState = processInstance.CurrentNodeId, 
                    ProcessInstanceId = cmd.ProcessInstanceId, 
                    Name = processDefinition.Name, 
                    ProcessType = processDefinition.Type, 
                    ContextMode = isUserActivityComplete ? "MODIFY" : "NORMAL"
                };

                result = workflowDefinition.ToGetContextResult(toGetContextParms);
            }
        }

        return result;
    }

    private static string FixXmlFormat(string? xmlString)
    {
        if (xmlString == null) xmlString = string.Empty;
        //correct any incorrect xml format
        xmlString = xmlString.Replace("\r\n", "");

        return xmlString;
    }
}