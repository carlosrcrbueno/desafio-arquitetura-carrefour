using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Controllers;
using Balance.Application.DTOs;
using Balance.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Tests.Controllers;

public class BalanceControllerTests
{
    [Fact]
    public async Task GetDailyBalance_DeveRetornar200()
    {
        var getDailyBalanceUseCaseMock = new Mock<IGetDailyBalanceUseCase>();
        getDailyBalanceUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<GetDailyBalanceRequest>()))
            .ReturnsAsync(new List<DailyBalanceDto>());

        var rebuildUseCaseMock = new Mock<IRebuildDailyBalancesUseCase>();

        var controller = new BalancesController(getDailyBalanceUseCaseMock.Object, rebuildUseCaseMock.Object);

        var result = await controller.GetDailyBalanceAsync(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task PostRebuild_DeveRetornar204()
    {
        var getDailyBalanceUseCaseMock = new Mock<IGetDailyBalanceUseCase>();
        var rebuildUseCaseMock = new Mock<IRebuildDailyBalancesUseCase>();

        var controller = new BalancesController(getDailyBalanceUseCaseMock.Object, rebuildUseCaseMock.Object);

        var result = await controller.RebuildAsync();

        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }
}
