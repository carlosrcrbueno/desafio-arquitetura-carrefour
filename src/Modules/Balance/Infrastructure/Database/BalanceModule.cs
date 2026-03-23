namespace Balance.Infrastructure.Database;

using Microsoft.Extensions.DependencyInjection;
using Shared.Database;
using Balance.Domain.Interfaces;
using Balance.Infrastructure.Repositories;

public static class BalanceModule
{
    public static IServiceCollection AddBalance(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        return services;
    }
}
