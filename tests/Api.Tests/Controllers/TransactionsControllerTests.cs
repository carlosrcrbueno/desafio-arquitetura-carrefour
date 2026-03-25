using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Enums;
using Transactions.Application.DTOs;
using Transactions.Application.UseCases;
using Xunit;

namespace Api.Tests.Controllers;

public class TransactionsControllerTests
{
    [Fact]
    public async Task PostTransactions_DeveRetornar201QuandoSucesso()
    {
        var useCaseMock = new Mock<ICreateTransactionUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CreateTransactionRequest>()))
            .ReturnsAsync(new CreateTransactionResponse { TransactionId = Guid.NewGuid() });

        var controller = new TransactionsController(useCaseMock.Object, null!);

        var request = new CreateTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100m,
            Type = TransactionType.Credit
        };

        var result = await controller.CreateAsync(request);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task PostTransactions_DeveRetornar400QuandoInvalido()
    {
        var useCaseMock = new Mock<ICreateTransactionUseCase>();
        var controller = new TransactionsController(useCaseMock.Object, null!);
        controller.ModelState.AddModelError("Amount", "Required");

        var request = new CreateTransactionRequest();

        var result = await controller.CreateAsync(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_DeveRetornar200ComDados()
    {
        var useCaseMock = new Mock<IGetTransactionsByAccountAndPeriodUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<GetTransactionsByAccountAndPeriodRequest>()))
            .ReturnsAsync(new List<TransactionDto>());

        var controller = new TransactionsController(null!, useCaseMock.Object);

        var result = await controller.GetAsync(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }
}
