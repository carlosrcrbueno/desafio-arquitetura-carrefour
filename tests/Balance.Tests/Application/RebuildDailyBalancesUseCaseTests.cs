namespace Balance.Tests.Application;

using Balance.Application.UseCases;
using Balance.Domain.Interfaces;
using Balance.Tests.Infrastructure;
using Moq;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Domain.Entities;
using Transactions.Domain.Interfaces;
using Xunit;

public class RebuildDailyBalancesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveReconstruirSaldos()
    {
        // Arrange
        var transactionRepositoryMock = new Mock<ITransactionRepository>();
        var dailyBalanceRepository = new FakeDailyBalanceRepository();

        var accountId = Guid.NewGuid();

        const int tenantId = 1;

        var transactions = new List<Transaction>
        {
            new(tenantId, Guid.NewGuid(), accountId, 100m, TransactionType.Credit, new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)),
            new(tenantId, Guid.NewGuid(), accountId, 50m, TransactionType.Debit, new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)),
            new(tenantId, Guid.NewGuid(), accountId, 25m, TransactionType.Credit, new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc))
        };

        var balanceAmountCredit = transactions.Where(x=>x.Type == TransactionType.Credit).Sum(x => x.Amount);
        var balanceAmountDebit = transactions.Where(x=>x.Type == TransactionType.Debit).Sum(x => x.Amount);
        var balanceAmount = balanceAmountCredit - balanceAmountDebit;

		transactionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(transactions);

        var useCase = new RebuildDailyBalancesUseCase(transactionRepositoryMock.Object, dailyBalanceRepository);

        // Act
        await useCase.ExecuteAsync().ConfigureAwait(false);

        // Assert
        var all = dailyBalanceRepository.GetAll();
        Assert.Equal(3, all.Count);

		var day1 = all.FirstOrDefault(b => b.Date.Date == new DateTime(2026, 1, 1, 10, 0, 0).Date);
		
		var day2 = all.FirstOrDefault(b => b.Date.Date == new DateTime(2026, 1, 2, 9, 0, 0).Date);

		Assert.NotNull(day1);
		Assert.NotNull(day2);

		Assert.Equal(100m, day1!.Balance);
		Assert.Equal(25m, day2!.Balance);
        Assert.Equal(75m, balanceAmount);


	}

    [Fact]
    public async Task ExecuteAsync_DeveLimparDadosAntesDeReprocessar()
    {
        // Arrange
        var transactionRepositoryMock = new Mock<ITransactionRepository>();
        var dailyBalanceRepositoryMock = new Mock<IDailyBalanceRepository>();

        var transactions = new List<Transaction>();

        transactionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(transactions);

        var useCase = new RebuildDailyBalancesUseCase(transactionRepositoryMock.Object, dailyBalanceRepositoryMock.Object);

        // Act
        await useCase.ExecuteAsync().ConfigureAwait(false);

        // Assert
        dailyBalanceRepositoryMock.Verify(r => r.DeleteAllAsync(), Times.Once);
    }
}
