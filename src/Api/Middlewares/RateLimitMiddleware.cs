using System.Net;
using System.Text.Json;
using StackExchange.Redis;

namespace Api.Middlewares;

public class RateLimitMiddleware
{
    private const int LimitPerMinute = 50;
    private const int WindowSeconds = 60;

    private readonly RequestDelegate _next;
    private readonly IDatabase _redis;

    public RateLimitMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
    {
        _next = next;
        _redis = redis.GetDatabase();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = context.Request.Path.ToString();
        var key = $"ratelimit:{ip}:{endpoint}";

        // incrementa contador no Redis e define TTL se for a primeira vez
        var count = await _redis.StringIncrementAsync(key);
        if (count == 1)
        {
            await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(WindowSeconds));
        }

        if (count > LimitPerMinute)
        {
            var ttl = await _redis.KeyTimeToLiveAsync(key) ?? TimeSpan.FromSeconds(WindowSeconds);
            var remainingSeconds = (int)Math.Ceiling(ttl.TotalSeconds);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var message = $"Seu Ip será liberado em {remainingSeconds} segundos, até lá suas requisições para este endpoint estarão impedidas.";
            var payload = JsonSerializer.Serialize(new { error = message });
            await context.Response.WriteAsync(payload);
            return;
        }

        await _next(context);
    }
}
