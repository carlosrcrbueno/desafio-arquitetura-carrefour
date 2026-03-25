namespace Transactions.Tests.Application;

using System;
using System.Threading.Tasks;
using Moq;
using Shared.Contracts;
using Shared.Messaging;
using Shared.Enums;
using Transactions.Application.DTOs;
using Transactions.Application.UseCases;
using Transactions.Domain.Entities;
using Transactions.Domain.Interfaces;
using Xunit;

public class CreateTransactionUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_DeveCriarTransacao()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var eventBusMock = new Mock<IEventBus>();

        var useCase = new CreateTransactionUseCase(repositoryMock.Object, eventBusMock.Object);

        var request = new CreateTransactionRequest
        {
            
            Amount = 100m,
            Type = TransactionType.Credit
        };

        // Act
        var response = await useCase.ExecuteAsync(request).ConfigureAwait(false);

        // Assert
        Assert.NotEqual(Guid.Empty, response.TransactionId);
        repositoryMock.Verify(r => r.InsertAsync(It.Is<Transaction>(t => t.Id == response.TransactionId)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ComAmountInvalido_DeveLancarExcecao()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var eventBusMock = new Mock<IEventBus>();

        var useCase = new CreateTransactionUseCase(repositoryMock.Object, eventBusMock.Object);

        var request = new CreateTransactionRequest
        {
            Amount = 0m,
            Type = TransactionType.Credit
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task.Run(async () => await useCase.ExecuteAsync(request).ConfigureAwait(false))).ConfigureAwait(false);
    }

    [Fact]
    public async Task ExecuteAsync_DevePublicarEvento()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var eventBusMock = new Mock<IEventBus>();

        var useCase = new CreateTransactionUseCase(repositoryMock.Object, eventBusMock.Object);

        var request = new CreateTransactionRequest
        {
            Amount = 100m,
            Type = TransactionType.Credit
        };

        // Act
        await useCase.ExecuteAsync(request).ConfigureAwait(false);

        // Assert
        eventBusMock.Verify(bus => bus.PublishAsync(It.IsAny<TransactionCreatedEvent>()), Times.Once);
    }
}
