namespace Balance.Tests.Application;

using System;
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
 public async Task ExecuteAsync_DeveRetornarSaldoDoDia()
    {
        // Arrange
       var repositoryMock = new Mock<IDailyBalanceRepository>();
        var useCase = new GetDailyBalanceUseCase(repositoryMock.Object);

        const int tenantId = 1;
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      var balance = new DailyBalance(tenantId, Guid.NewGuid(), start, 10000L);

        repositoryMock
            .Setup(r => r.GetByTenantAndDateAsync(tenantId, start))
            .ReturnsAsync(balance);

        var request = new GetDailyBalanceRequest
        {
            TenantId = tenantId,
          StartDate = start,
            EndDate = start
        };

        // Act
     var result = await useCase.ExecuteAsync(request).ConfigureAwait(false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result!.TenantId);
        Assert.Equal(start, result.Date);
        Assert.Equal(100m, result.Balance);
    }

    [Fact]
    public async Task ExecuteAsync_SemDados_DeveRetornarNulo()
    {
        // Arrange
       var repositoryMock = new Mock<IDailyBalanceRepository>();
        var useCase = new GetDailyBalanceUseCase(repositoryMock.Object);

        const int tenantId = 1;
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        repositoryMock
            .Setup(r => r.GetByTenantAndDateAsync(tenantId, start))
            .ReturnsAsync((DailyBalance?)null);

        var request = new GetDailyBalanceRequest
        {
            TenantId = tenantId,
            StartDate = start,
            EndDate = end
        };

        // Act
     var result = await useCase.ExecuteAsync(request).ConfigureAwait(false);

        // Assert
        Assert.Null(result);
    }
}
