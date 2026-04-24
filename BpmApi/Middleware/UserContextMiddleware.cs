using BpmInfrastructure.Context;

namespace BpmApi.Middleware;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, UserContext userContext)
    {
        userContext.User = context.Request.Headers["X-User"].ToString();
        userContext.Company = context.Request.Headers["X-Company"].ToString();
        userContext.CurrentHttpContext = $"{context.Request.Scheme}://{context.Request.Host}";
        userContext.Abi = "";

        context.Items["User"] = userContext.User;
        context.Items["Company"] = userContext.Company;

        context.Items["CurrentHttpContext"] = userContext.CurrentHttpContext;
        context.Items["Abi"] = userContext.Abi;

        await _next(context);
    }
}