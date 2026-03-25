namespace Transactions.Tests.Infrastructure;

using System;
using System.Threading.Tasks;
using Shared.Enums;
using Transactions.Domain.Entities;
using Xunit;

public class TransactionRepositoryTests
{
    [Fact]
    public async Task InsertAsync_DeveSalvarNaParticaoCorreta()
    {
        // Arrange
       var repository = new FakeTransactionRepository();
        var createdAt = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);
        var transaction = new Transaction(
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            TransactionType.Credit,
            createdAt);

        // Act
        await repository.InsertAsync(transaction).ConfigureAwait(false);

        // Assert
        Assert.Contains("transactions_2026_01", repository.UsedTables);
    }

    [Fact]
    public async Task GetByAccountAndPeriodAsync_DeveRetornarDadosDoPeriodo()
    {
        // Arrange
       var repository = new FakeTransactionRepository();
        var accountId = Guid.NewGuid();
        const int tenantId = 1;

        var t1 = new Transaction(tenantId, Guid.NewGuid(), accountId, 50m, TransactionType.Credit, new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));
        var t2 = new Transaction(tenantId, Guid.NewGuid(), accountId, 75m, TransactionType.Debit, new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        var t3 = new Transaction(tenantId, Guid.NewGuid(), accountId, 25m, TransactionType.Credit, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        await repository.InsertAsync(t1).ConfigureAwait(false);
        await repository.InsertAsync(t2).ConfigureAwait(false);
        await repository.InsertAsync(t3).ConfigureAwait(false);

        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 31, 23, 59, 59);

        // Act
        var result = await repository.GetByAccountAndPeriodAsync(accountId, start, end).ConfigureAwait(false);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, t => t.CreatedAt.Month != 1);
    }

    [Fact]
    public async Task GetByAccountAndPeriodAsync_DeveConsultarMultiplasParticoes()
    {
        // Arrange
       var repository = new FakeTransactionRepository();
        var accountId = Guid.NewGuid();
        const int tenantId = 1;

        var janTransaction = new Transaction(tenantId, Guid.NewGuid(), accountId, 50m, TransactionType.Credit, new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc));
        var febTransaction = new Transaction(tenantId, Guid.NewGuid(), accountId, 75m, TransactionType.Credit, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        await repository.InsertAsync(janTransaction).ConfigureAwait(false);
        await repository.InsertAsync(febTransaction).ConfigureAwait(false);

        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 2, 28, 23, 59, 59);

        // Act
        var result = await repository.GetByAccountAndPeriodAsync(accountId, start, end).ConfigureAwait(false);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("transactions_2026_01", repository.UsedTables);
        Assert.Contains("transactions_2026_02", repository.UsedTables);
    }
}
