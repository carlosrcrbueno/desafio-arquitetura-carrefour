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
        var hasIdempotenceKey = !string.IsNullOrWhiteSpace(request.IdempotenceKey);
		var idempotenceKey = hasIdempotenceKey
			? request.IdempotenceKey
			: Guid.NewGuid().ToString("N");

		var transaction = new Transaction(
			request.TenantId,
			Guid.NewGuid(),
			request.AccountId,
         (long)Math.Round(request.Amount * 100m, MidpointRounding.AwayFromZero),
           request.Type,
			DateTime.UtcNow,
			idempotenceKey);

        var inserted = await _transactionRepository.InsertAsync(transaction).ConfigureAwait(false);

		// Only publish event when transaction was effectively inserted (not an idempotent replay)
		if (inserted)
		{
			var @event = new TransactionCreatedEvent
			{
				TenantId = transaction.TenantId,
				TransactionId = transaction.Id,
				AccountId = transaction.AccountId,
				Amount = transaction.Amount,
				Type = (Shared.Enums.TransactionType)transaction.Type,
				CreatedAt = transaction.CreatedAt
			};

			await _eventBus.PublishAsync(@event).ConfigureAwait(false);
		}

		return new CreateTransactionResponse
		{
			TransactionId = transaction.Id,
			AccountId = transaction.AccountId,
			IsNew = inserted,
		};
	}
}
