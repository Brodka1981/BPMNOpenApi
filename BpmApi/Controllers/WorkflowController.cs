using BpmApplication.Commands;
using BpmApplication.DTO;
using BpmApplication.Errors;
using BpmApplication.Services.Interfaces;
using BpmInfrastructure.Context;
using BpmWebApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BpmApi.Controllers;

[ApiController]
[Route("api/workflow")]
public class WorkflowController : BpmControllerBase
{
    private readonly IWorkflowAppService _service;
    private readonly UserContext _userContext;

    public WorkflowController(IWorkflowAppService service, UserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

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
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> StartProcess([FromBody] StartProcessRequest request)
    {
        var command = new StartProcessCommand
        {
            DefinitionId = request.DefinitionId,
            Variables = request.Variables,
            //User = _userContext.User,
            User = "fc0429",
            //Company = _userContext.Company
            Company = "99900"
        };

        var result = await _service.StartProcessAsync(command);

        if (!result.Success)
            return MapError(result.Error!);

        StartProcessResponse? response = null;

        if (result != null)
        {
            response = new StartProcessResponse
            {
                ProcessId = result.Value!.ProcessId
            };
        }

        return Ok(response);
    }

    /// <summary>
    /// Get Context
    /// </summary>
    /// <param name="processInstanceId"></param>
    /// <returns></returns>
    // 1️⃣ CONTEXT
    [HttpGet("context/{processInstanceId}")]
    public async Task<IActionResult> GetContext(long processInstanceId)
    {
        var command = new GetContextCommand() 
        {
            ProcessInstanceId = processInstanceId,
            User = _userContext.User,
            Company = _userContext.Company    
        };
        var result = await _service.GetContextAsync(command);

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
    // Gestione degli errori specificvata nel BpmControllerBase
    [HttpGet("definitions")]
    [ProducesResponseType(typeof(IEnumerable<WorkflowDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListDefinitions([FromQuery] string? categoria)
    {
        var command = new GetDefinitionsCommand
        {
            User = _userContext.User,
            Company = _userContext.Company,
            Category = categoria
        };

        var result = await _service.GetDefinitionsAsync(command);
        if (!result.Success)
            return MapError(result.Error!);

        return Ok(result.Value);
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
