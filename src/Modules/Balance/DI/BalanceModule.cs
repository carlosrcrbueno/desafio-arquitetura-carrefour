namespace Balance.DI;

using Balance.Application.UseCases;
using Balance.Domain.Interfaces;
using Balance.Infrastructure.Database;
using Balance.Infrastructure.EventHandlers;
using Balance.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Shared.Database;

public static class BalanceModule
{
	public static IServiceCollection AddBalance(this IServiceCollection services, string connectionString)
	{
		if (services is null)
		{
			throw new ArgumentNullException(nameof(services));
		}

		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new ArgumentException("Connection string must be provided.", nameof(connectionString));
		}

		// Balance module registrations.
		services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
		services.AddScoped<IGetDailyBalanceUseCase, GetDailyBalanceUseCase>();
		services.AddScoped<IRebuildDailyBalancesUseCase, RebuildDailyBalancesUseCase>();

		// Event handler for transaction-created events that updates daily balances.
		services.AddScoped<TransactionCreatedEventHandler>();

		return services;
	}
}