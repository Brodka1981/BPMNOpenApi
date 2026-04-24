using BpmApplication.Errors;
using BpmApplication.Messages;
using Microsoft.AspNetCore.Mvc;

namespace BpmApi.Controllers;

public abstract class BpmControllerBase : ControllerBase
{
    protected string CurrentUser =>
        HttpContext.Items["User"]?.ToString() ?? "unknown";

    protected string CurrentCompany =>
        HttpContext.Items["Company"]?.ToString() ?? "default";

    // Serve per la gestione degli errori che vengono dai vari sottotipi di Controllers
    protected IActionResult MapMessage(ApiMessage error) =>
        error.Code switch
        {
            ErrorCodes.NotFound => NotFound(error),
            ErrorCodes.Invalid => BadRequest(error),
            ErrorCodes.Unauthorized => StatusCode(StatusCodes.Status403Forbidden, error),
            ErrorCodes.Conflict => Conflict(error),
            _ => StatusCode(StatusCodes.Status500InternalServerError, error)
        };
}