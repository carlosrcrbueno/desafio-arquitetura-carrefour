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
        var balance = new DailyBalance(Guid.NewGuid(), new DateOnly(2026, 1, 1), 100m);

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
        var date = new DateOnly(2026, 1, 1);

        var original = new DailyBalance(accountId, date, 100m);
        await repository.UpsertAsync(original).ConfigureAwait(false);

        var updated = new DailyBalance(accountId, date, 150m);

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

        await repository.UpsertAsync(new DailyBalance(accountId, new DateOnly(2026, 1, 1), 100m)).ConfigureAwait(false);
        await repository.UpsertAsync(new DailyBalance(accountId, new DateOnly(2026, 1, 2), 150m)).ConfigureAwait(false);
        await repository.UpsertAsync(new DailyBalance(Guid.NewGuid(), new DateOnly(2026, 1, 1), 999m)).ConfigureAwait(false);

        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 2);

        // Act
        var result = await repository.GetByAccountAndPeriodAsync(accountId, start, end).ConfigureAwait(false);

        // Assert
        Assert.Equal(2, result.Count);
    }
}
