using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Balance.Application.UseCases;
using Balance.Domain.Entities;
using Balance.Domain.Interfaces;
using Moq;
using Shared.Contracts;
using Shared.Enums;
using Transactions.Domain.Entities;
using Transactions.Domain.Interfaces;
using Xunit;

namespace Balance.Tests.Integration;

public class ReprocessingFlowTests
{
    [Fact]
    public async Task FalhaNoReadModel_DeveSerCorrigidaComReprocessamento()
    {
        // Arrange
        var transactionRepositoryMock = new Mock<ITransactionRepository>();
        var dailyBalanceRepositoryMock = new Mock<IDailyBalanceRepository>();

     const int tenantId = 1;
        var accountId = Guid.NewGuid();
        var date = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        var transactions = new List<Transaction>
        {
            new Transaction(
                tenantId,
                Guid.NewGuid(),
                accountId,
                100m,
                (TransactionType)Shared.Enums.TransactionType.Credit,
                date),
            new Transaction(
                tenantId,
                Guid.NewGuid(),
                accountId,
                50m,
                (TransactionType)Shared.Enums.TransactionType.Debit,
                date)
        };

        transactionRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(transactions);

        var useCase = new RebuildDailyBalancesUseCase(
            transactionRepositoryMock.Object,
            dailyBalanceRepositoryMock.Object
        );

        // Act
        await useCase.ExecuteAsync();

        // Assert
        dailyBalanceRepositoryMock.Verify(x => x.DeleteAllAsync(), Times.Once);
        dailyBalanceRepositoryMock.Verify(x => x.UpsertAsync(It.Is<DailyBalance>(b =>
            b.AccountId == accountId &&
            b.Balance == 50m // 100 credit - 50 debit
        )), Times.AtLeastOnce);
    }
}
