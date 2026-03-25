namespace Transactions.DI;

using Microsoft.Extensions.DependencyInjection;
using Shared.Database;
using Transactions.Application.UseCases;
using Transactions.Domain.Interfaces;
using Transactions.Infrastructure.Database;
using Transactions.Infrastructure.Repositories;

public static class TransactionsModule
{
	public static IServiceCollection AddTransactions(this IServiceCollection services, string connectionString)
	{
		// Factory de conexão (já retornando NpgsqlConnection)
		services.AddScoped<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

		// Repositório de transações
		services.AddScoped<ITransactionRepository, TransactionRepository>();

		// Use cases
		services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
		services.AddScoped<IGetTransactionsByAccountAndPeriodUseCase, GetTransactionsByAccountAndPeriodUseCase>();

		return services;
	}
}