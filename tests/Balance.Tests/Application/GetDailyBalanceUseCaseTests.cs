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

        const int tenantId = 1;
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        var balances = new List<DailyBalance>
        {
            new(tenantId, Guid.NewGuid(), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 100m),
            new(tenantId, Guid.NewGuid(), new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), 150m)
        };

        repositoryMock
            .Setup(r => r.GetByTenantAndPeriodAsync(tenantId, start, end))
            .ReturnsAsync(balances);

        var request = new GetDailyBalanceRequest
        {
            TenantId = tenantId,
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

        const int tenantId = 1;
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        repositoryMock
            .Setup(r => r.GetByTenantAndPeriodAsync(tenantId, start, end))
            .ReturnsAsync(new List<DailyBalance>());

        var request = new GetDailyBalanceRequest
        {
            TenantId = tenantId,
            StartDate = start,
            EndDate = end
        };

        // Act
        var result = await useCase.ExecuteAsync(request).ConfigureAwait(false);

        // Assert
        Assert.Empty(result);
    }
}
