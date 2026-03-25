using Moq;
using Shared.Contracts;
using Shared.Messaging;
using Transactions.Application.DTOs;
using Transactions.Application.UseCases;
using Transactions.Domain.Entities;
using Transactions.Domain.Interfaces;

namespace Transactions.Tests.Integration;

public class TransactionFlowTests
{
    [Fact]
    public async Task CreateTransaction_DeveAtualizarLedgerEReadModel()
    {
        // Arrange
        var transactionRepositoryMock = new Mock<ITransactionRepository>();
        var eventBusMock = new Mock<IEventBus>();

        Transaction? savedTransaction = null;
        eventBusMock
            .Setup(x => x.PublishAsync(It.IsAny<TransactionCreatedEvent>()))
            .Returns(Task.CompletedTask);

        transactionRepositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => savedTransaction = t)
            .Returns(Task.CompletedTask);

        var useCase = new CreateTransactionUseCase(
            transactionRepositoryMock.Object,
            eventBusMock.Object
        );

        var request = new CreateTransactionRequest
        {
          TenantId = 1,
            Amount = 100m,
            Type = Shared.Enums.TransactionType.Credit
        };

        // Act
        var response = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, response.TransactionId);
        Assert.NotNull(savedTransaction);
        Assert.Equal(response.TransactionId, savedTransaction!.Id);

        eventBusMock.Verify(
         x => x.PublishAsync(It.Is<TransactionCreatedEvent>(e =>
                e.TransactionId == response.TransactionId &&
                e.Amount == request.Amount &&
                e.Type == request.Type
            )),
            Times.Once
        );
    }
}
