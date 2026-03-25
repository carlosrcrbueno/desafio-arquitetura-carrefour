namespace Balance.Tests.Infrastructure;

using System;
using System.Threading.Tasks;
using Balance.Domain.Entities;
using Xunit;

public class DailyBalanceRepositoryTests
{
    [Fact]
    public async Task UpsertAsync_DeveInserirNovoSaldo()
    {
        // Arrange
        var repository = new FakeDailyBalanceRepository();
        var balance = new DailyBalance(1, Guid.NewGuid(), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10000L);

        // Act
        await repository.UpsertAsync(balance).ConfigureAwait(false);

        // Assert
        var all = repository.GetAll();
        Assert.Single(all);
        Assert.Equal(100m, all[0].Balance);
    }

    [Fact]
    public async Task UpsertAsync_DeveAtualizarSaldoExistente()
    {
        // Arrange
      var repository = new FakeDailyBalanceRepository();
        var accountId = Guid.NewGuid();
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

      var original = new DailyBalance(1, accountId, date, 10000L);
        await repository.UpsertAsync(original).ConfigureAwait(false);

     var updated = new DailyBalance(1, accountId, date, 15000L);

        // Act
        await repository.UpsertAsync(updated).ConfigureAwait(false);

        // Assert
        var all = repository.GetAll();
        Assert.Single(all);
        Assert.Equal(150m, all[0].Balance);
    }

    [Fact]
    public async Task GetByAccountAndPeriodAsync_DeveRetornarSaldos()
    {
        // Arrange
      var repository = new FakeDailyBalanceRepository();
        var accountId = Guid.NewGuid();
        const int tenantId = 1;

       await repository.UpsertAsync(new DailyBalance(tenantId, accountId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10000L)).ConfigureAwait(false);
        await repository.UpsertAsync(new DailyBalance(tenantId, accountId, new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), 15000L)).ConfigureAwait(false);
        await repository.UpsertAsync(new DailyBalance(2, Guid.NewGuid(), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 99900L)).ConfigureAwait(false);

       var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 2, 23, 59, 59, DateTimeKind.Utc);

        // Act
      var result = await repository.GetByTenantAndPeriodAsync(tenantId, start, end).ConfigureAwait(false);

        // Assert
        Assert.Equal(2, result.Count);
    }
}
