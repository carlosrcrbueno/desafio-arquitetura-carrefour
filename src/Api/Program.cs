using Api.EventBus;
using Api.Filter;
using Api.Middlewares;
using Balance.DI;
using Balance.Infrastructure.EventHandlers;
using Shared.Env;
using Shared.Messaging;
using StackExchange.Redis;
using Transactions.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
	options.Filters.Add<TenantInjectionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection not configured");

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
	?? throw new InvalidOperationException("ConnectionStrings:Redis not configured");

builder.Services.AddSingleton<IConnectionMultiplexer>(
	_ => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

builder.Services.AddTransactions(connectionString);
builder.Services.AddBalance(connectionString);

var app = builder.Build();

// Wire up event subscriptions.
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<Shared.Contracts.TransactionCreatedEvent>(async @event =>
{
	using var scope = app.Services.CreateScope();
	var handler = scope.ServiceProvider.GetRequiredService<TransactionCreatedEventHandler>();
	await handler.HandleAsync(@event).ConfigureAwait(false);
});

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<AuthorizationMockMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();