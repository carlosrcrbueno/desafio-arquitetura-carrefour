namespace Transactions.Tests.Application;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Shared.Enums;
using Transactions.Application.DTOs;
using Transactions.Application.UseCases;
using Transactions.Domain.Entities;
using Transactions.Domain.Interfaces;
using Xunit;

public class GetTransactionsByAccountAndPeriodUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveRetornarTransacoesDoPeriodo()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var useCase = new GetTransactionsByAccountAndPeriodUseCase(repositoryMock.Object);

        var accountId = Guid.NewGuid();
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 31);

        const int tenantId = 1;
        var transactions = new List<Transaction>
        {
            new(tenantId, Guid.NewGuid(), accountId, 50m, TransactionType.Credit, new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)),
            new(tenantId, Guid.NewGuid(), accountId, 75m, TransactionType.Debit, new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc))
        };

        repositoryMock
            .Setup(r => r.GetByAccountAndPeriodAsync(accountId, start, end))
            .ReturnsAsync(transactions);

        var request = new GetTransactionsByAccountAndPeriodRequest
        {
            AccountId = accountId,
            StartDate = start,
            EndDate = end
        };

        // Act
        var result = await useCase.ExecuteAsync(request).ConfigureAwait(false);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_SemDados_DeveRetornarListaVazia()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var useCase = new GetTransactionsByAccountAndPeriodUseCase(repositoryMock.Object);

        var accountId = Guid.NewGuid();
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 31);

        repositoryMock
            .Setup(r => r.GetByAccountAndPeriodAsync(accountId, start, end))
            .ReturnsAsync(new List<Transaction>());

        var request = new GetTransactionsByAccountAndPeriodRequest
        {
            AccountId = accountId,
            StartDate = start,
            EndDate = end
        };

        // Act
        var result = await useCase.ExecuteAsync(request).ConfigureAwait(false);

        // Assert
        Assert.Empty(result);
    }
}
