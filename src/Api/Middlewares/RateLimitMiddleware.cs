using System.Net;
using System.Text.Json;
using StackExchange.Redis;

namespace Api.Middlewares;

// Middleware de rate limit baseado em Redis com:
// - limite fixo de 50 requisições por segundo
// - bloqueio ("freeze") temporário de 10s após estouro do limite
public class RateLimitMiddleware
{
	// Limite máximo de requisições permitidas por segundo (por IP + rota)
	private const int LimitPerSecond = 50;

	// Tempo de bloqueio após o limite ser excedido
	private static readonly TimeSpan BlockDuration = TimeSpan.FromSeconds(10);

	private readonly RequestDelegate _next;
	private readonly IDatabase _redis;

	public RateLimitMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
	{
		_next = next;
		_redis = redis.GetDatabase();
	}

	public async Task InvokeAsync(HttpContext context)
	{
		// Identificador do cliente (IP) e da rota para compor a chave do rate limit
		var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		var endpoint = context.Request.Path.ToString();

		// Chave de bloqueio global (freeze) por IP + rota.
		// Enquanto existir no Redis, todas as requisições recebem 429 imediatamente.
		var blockKey = $"ratelimit:block:{ip}:{endpoint}";

		// Se já existe bloqueio ativo, não conta a requisição, apenas retorna 429.
		if (await _redis.KeyExistsAsync(blockKey).ConfigureAwait(false))
		{
			await WriteTooManyRequestsAsync(context);
			return;
		}

		// Segundo atual em epoch (janela fixa de 1 segundo).
		// Cada segundo forma um "bucket" independente.
		var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

		// Chave do contador por segundo para este IP + rota.
		// Ex.: ratelimit:127.0.0.1:/transactions:1711380000
		var key = $"ratelimit:{ip}:{endpoint}:{now}";

		// Incrementa o contador de requisições deste bucket.
		// O valor retornado é o total de requisições neste segundo para IP + rota.
		var currentCount = await _redis.StringIncrementAsync(key).ConfigureAwait(false);

		// Define TTL para o bucket atual.
		// Só precisamos desse contador enquanto o segundo estiver "vivo",
		// então um TTL curto é suficiente.
		if (currentCount == 1)
		{
			await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(3)).ConfigureAwait(false);
		}

		// Se o total deste segundo passou do limite, ativa bloqueio e retorna 429.
		if (currentCount > LimitPerSecond)
		{
			// Marca bloqueio de curto prazo por IP + rota.
			// Enquanto o TTL desta chave não expirar, todas as requisições serão bloqueadas.
			await _redis.StringSetAsync(blockKey, "1", BlockDuration).ConfigureAwait(false);

			await WriteTooManyRequestsAsync(context);
			return;
		}

		// Dentro do limite: segue o pipeline normal
		await _next(context);
	}

	// Resposta padrão de Too Many Requests com payload JSON consistente.
	private static async Task WriteTooManyRequestsAsync(HttpContext context)
	{
		context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
		context.Response.ContentType = "application/json";

		var message = $"Limite de {LimitPerSecond} requisições por segundo excedido.";
		var payload = JsonSerializer.Serialize(new { error = message });
		await context.Response.WriteAsync(payload);
	}
}