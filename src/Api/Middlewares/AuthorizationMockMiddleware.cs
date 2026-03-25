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
        // Allow unauthenticated access to Swagger endpoints so documentation loads without a token.
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.ContainsKey("Authorization") ||
            string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Unauthorized\"}");
            return;
        }

        // Resolve TenantId from header X-Tenant-Id as int and store it for controllers/use cases.
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader)
            && int.TryParse(tenantHeader, out var tenantId)
            && tenantId > 0)
        {
            context.Items["TenantId"] = tenantId;
        }

        await _next(context);
    }
}
