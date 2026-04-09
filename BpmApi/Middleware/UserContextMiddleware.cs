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
        context.Items["User"] = userContext.User;
        context.Items["Company"] = userContext.Company;


        await _next(context);
    }
}