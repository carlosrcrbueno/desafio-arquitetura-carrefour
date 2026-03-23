namespace Transactions.Application.UseCases;

using System;
using System.Threading.Tasks;
using Shared.Contracts;
using Shared.Messaging;
using Transactions.Application.DTOs;
using Transactions.Domain.Entities;
using Transactions.Domain.Interfaces;

public class CreateTransactionUseCase : ICreateTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IEventBus _eventBus;

    public CreateTransactionUseCase(ITransactionRepository transactionRepository, IEventBus eventBus)
    {
        _transactionRepository = transactionRepository;
        _eventBus = eventBus;
    }

    public async Task<CreateTransactionResponse> ExecuteAsync(CreateTransactionRequest request)
    {
        var transaction = new Transaction(
            Guid.NewGuid(),
            request.AccountId,
            request.Amount,
            request.Type,
            DateTime.UtcNow);

        await _transactionRepository.InsertAsync(transaction).ConfigureAwait(false);

        var @event = new TransactionCreatedEvent
        {
            TransactionId = transaction.Id,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            Type = (Shared.Enums.TransactionType)transaction.Type,
            CreatedAt = transaction.CreatedAt
        };

        await _eventBus.PublishAsync(@event).ConfigureAwait(false);

        return new CreateTransactionResponse
        {
            TransactionId = transaction.Id
        };
    }
}
