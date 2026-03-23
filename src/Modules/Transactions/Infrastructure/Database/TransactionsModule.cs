namespace Transactions.Infrastructure.Database;

using Microsoft.Extensions.DependencyInjection;
using Shared.Database;
using Transactions.Domain.Interfaces;
using Transactions.Infrastructure.Repositories;

public static class TransactionsModule
{
    public static IServiceCollection AddTransactions(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        return services;
    }
}
