using BpmApplication.Commands;
using BpmApplication.DTO;
using BpmApplication.Errors;
using BpmApplication.Messages;
using BpmApplication.Services.Interfaces;
using BpmInfrastructure.Context;
using BpmWebApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BpmApi.Controllers;

[ApiController]
[Route("api/process")]
public class WorkflowController(IWorkflowAppService service, UserContext userContext) : BpmControllerBase
{
    private readonly IWorkflowAppService _service = service;
    private readonly UserContext _userContext = userContext;

    /// <summary>
    /// Avvia un nuovo processo workflow a partire da una definizione BPMN.
    /// </summary>
    /// <remarks>
    /// Questo endpoint:
    /// - legge la definizione BPMN associata a <c>DefinitionId</c>
    /// - verifica che l'utente corrente abbia i permessi per avviare il processo
    /// - crea una nuova istanza del workflow
    /// - restituisce l'ID del processo creato
    ///
    /// Esempio di richiesta:    /// 
    ///     POST /api/workflow/start
    ///     {
    ///         "definitionId": "onboarding",
    ///         "variables": {
    ///             "Cliente":"Roberto Rossi",
    ///             "BusinessKey": "A100023"
    ///         }
    ///     }
    ///
    /// Esempio di risposta:    ///
    ///     {
    ///         "processId": 12345
    ///     }
    ///
    /// </remarks>
    /// <param name="request">Dati necessari per avviare il processo.</param>
    /// <returns>ID del processo creato.</returns>
    /// <response code="200">Processo avviato correttamente.</response>
    /// <response code="400">Dati non validi.</response>
    /// <response code="403">Utente autenticato ma non autorizzato all'avvio del processo.</response>
    /// <response code="404">Definizione BPMN non trovata.</response>
    /// <response code="500">Errore interno del server.</response>
    [HttpPost("start")]
    [ProducesResponseType(typeof(StartProcessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> StartProcess([FromBody] StartProcessRequest request)
    {
        var command = new StartProcessCommand
        {
            ProcessType = request.ProcessType,
            Variables = request.Variables,
            //User = _userContext.User,
            User = "fc0429",
            //Company = _userContext.Company
            Company = "99900"
        };

        var result = await _service.StartProcessAsync(command);

        if (!result.Success)
            return MapMessage(result.Message!);

        return Ok(result);
    }

    /// <summary>
    /// Get Context
    /// </summary>
    /// <param name="processInstanceId"></param>
    /// <returns></returns>
    // 1️⃣ CONTEXT
    [HttpGet("context/{processInstanceId}")]
    [ProducesResponseType(typeof(StartProcessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetContext(long processInstanceId)
    {
        var command = new GetContextCommand() 
        {
            ProcessInstanceId = processInstanceId,
            //User = _userContext.User,
            User = "fc0429",
            //Company = _userContext.Company,
            Company = "99900",
            CurrentHttpContext = _userContext.CurrentHttpContext,
            Abi = _userContext.Abi
        };

        var result = await _service.GetContextAsync(command);

        if (!result.Success)
            return MapMessage(result.Message!);

        return Ok(result);
    }

    //// 2️⃣ ACTION
    //[HttpPost("action")]
    //public async Task<IActionResult> ExecuteAction([FromBody] WorkflowActionRequest request)
    //{
    //    var result = await _service.ExecuteActionAsync(request, CurrentUser, CurrentCompany);
    //    return Ok(result);
    //}

    //// 3️⃣ VALIDATE
    //[HttpPost("validate")]
    //public async Task<IActionResult> Validate([FromBody] WorkflowValidateRequest request)
    //{
    //    var result = await _service.ValidateActionAsync(request, CurrentUser, CurrentCompany);
    //    return Ok(result);
    //}

    //// 4️⃣ GET VARIABLES
    //[HttpGet("variables/{processId}")]
    //public async Task<IActionResult> GetVariables(Guid processId)
    //{
    //    var result = await _service.GetVariablesAsync(processId, CurrentUser, CurrentCompany);
    //    return Ok(result);
    //}

    /// <summary>
    /// List Definitions
    /// </summary>
    /// <param name="categoria"></param>
    /// <returns></returns>
    // 6️⃣ LIST DEFINITIONS
    // Endpoint che restituisce le definizioni BPMN disponibili
    // Gestione degli errori specificata nel BpmControllerBase
    [HttpGet("definitions")]
    [ProducesResponseType(typeof(IEnumerable<WorkflowDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListDefinitions([FromQuery] string? categoria)
    {
        var command = new GetDefinitionsCommand
        {
            //User = _userContext.User,
            User = "fc0429",
            //Company = _userContext.Company,
            Company = "99900",
            Category = categoria
        };

        var result = await _service.GetDefinitionsAsync(command);
        if (!result.Success)
            return MapMessage(result.Message!);

        return Ok(result);
    }
    /// <summary>
    /// Restituisce tutte le ProcessInstance relative ai filtri di ricerca specificati nella request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(IEnumerable<SearchProcessDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessage), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchProcess([FromBody] SearchProcessRequest request)
    {
        var command = new SearchProcessCommand
        {
            DefinitionType = request.DefinitionType,
            IdState = request.IdState,
            IsClosed = request.IsClosed,
            Category = request.Category,
            Filters = request.Filters,
            Columns = request.Columns,
            Sort = request.Sort,
            ExportType = request.ExportType,
            Page = request.Page,
            Size = request.Size
        };

        var result = await _service.SearchProcessAsync(command);
        if (!result.Success)
            return MapMessage(result.Message!);

        return Ok(result);
    }



    //// 7️⃣ GET BPMN FILE
    //[HttpGet("definitions/{idWorkflow}/bpmn")]
    //public async Task<IActionResult> GetBpmn(string idWorkflow)
    //{
    //    var result = await _service.GetBpmnAsync(idWorkflow, CurrentUser, CurrentCompany);
    //    return File(result.Content, "application/octet-stream", result.FileName);
    //}

    //// 8️⃣ PENDING PROCESSES
    //[HttpPost("pending")]
    //public async Task<IActionResult> GetPending([FromBody] PendingProcessFilter filter)
    //{
    //    var result = await _service.GetPendingProcessesAsync(filter, CurrentUser, CurrentCompany);
    //    return Ok(result);
    //}
    //}
}