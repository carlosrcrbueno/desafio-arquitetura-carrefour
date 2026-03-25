using System.Net;
using System.Text.Json;
using StackExchange.Redis;

namespace Api.Middlewares;

public class RateLimitMiddleware
{
    private const int LimitPerSecond = 50;
    private const int WindowSeconds = 30;
    private const int WindowLimit = LimitPerSecond * WindowSeconds;

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

        // segundo atual (epoch)
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // identificador de janela deslizante baseado em WindowSeconds
        var windowId = now / WindowSeconds;
        var key = $"ratelimit:{ip}:{endpoint}:{windowId}";

        // campo do hash referente ao segundo atual dentro da janela
        var field = (now % WindowSeconds).ToString();

        // incrementa contador do segundo atual
        var _ = await _redis.HashIncrementAsync(key, field);

        // define TTL um pouco maior que a janela para permitir leitura dos últimos segundos
        await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(WindowSeconds + 2));

        // obtém todos os contadores de segundos desta janela
        var entries = await _redis.HashGetAllAsync(key);
        long totalInWindow = 0;
        foreach (var entry in entries)
        {
            if (long.TryParse(entry.Name!, out var secondOffset) &&
                secondOffset >= 0 && secondOffset < WindowSeconds)
            {
                if (long.TryParse(entry.Value!, out var value))
                {
                    totalInWindow += value;
                }
            }
        }

        if (totalInWindow > WindowLimit)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var message = $"Limite de {LimitPerSecond} requisições por segundo (janela de {WindowSeconds}s) excedido.";
            var payload = JsonSerializer.Serialize(new { error = message });
            await context.Response.WriteAsync(payload);
            return;
        }

        await _next(context);
    }
}
