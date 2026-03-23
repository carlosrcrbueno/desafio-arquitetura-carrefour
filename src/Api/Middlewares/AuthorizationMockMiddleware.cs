namespace Api.Middlewares;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class AuthorizationMockMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMockMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Authorization") ||
            string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Unauthorized\"}");
            return;
        }

        await _next(context);
    }
}
