namespace Balance.Tests.Application;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Balance.Application.DTOs;
using Balance.Application.UseCases;
using Balance.Domain.Entities;
using Balance.Domain.Interfaces;
using Moq;
using Xunit;

public class GetDailyBalanceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveRetornarSaldos()
    {
        // Arrange
        var repositoryMock = new Mock<IDailyBalanceRepository>();
        var useCase = new GetDailyBalanceUseCase(repositoryMock.Object);

        var accountId = Guid.NewGuid();
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 31);

        var balances = new List<DailyBalance>
        {
            new(accountId, new DateOnly(2026, 1, 1), 100m),
            new(accountId, new DateOnly(2026, 1, 2), 150m)
        };

        repositoryMock
            .Setup(r => r.GetByAccountAndPeriodAsync(accountId, start, end))
            .ReturnsAsync(balances);

        var request = new GetDailyBalanceRequest
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
        var repositoryMock = new Mock<IDailyBalanceRepository>();
        var useCase = new GetDailyBalanceUseCase(repositoryMock.Object);

        var accountId = Guid.NewGuid();
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 31);

        repositoryMock
            .Setup(r => r.GetByAccountAndPeriodAsync(accountId, start, end))
            .ReturnsAsync(new List<DailyBalance>());

        var request = new GetDailyBalanceRequest
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
