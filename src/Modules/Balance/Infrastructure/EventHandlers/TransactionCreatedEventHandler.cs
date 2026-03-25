namespace Balance.Infrastructure.EventHandlers;

using System;
using System.Linq;
using System.Threading.Tasks;
using Balance.Domain.Entities;
using Balance.Domain.Interfaces;
using Shared.Contracts;
using Shared.Enums;

public class TransactionCreatedEventHandler
{
	private readonly IDailyBalanceRepository _dailyBalanceRepository;

	public TransactionCreatedEventHandler(IDailyBalanceRepository dailyBalanceRepository)
	{
		_dailyBalanceRepository = dailyBalanceRepository;
	}

	public async Task HandleAsync(TransactionCreatedEvent @event)
	{
		if (@event is null)
		{
			throw new ArgumentNullException(nameof(@event));
		}

		// For now, assume a single global tenant id. In a real system, TenantId should be part of the event.
		var tenantId = @event.TenantId;

		var accountId = @event.AccountId;
		var dateUtc = @event.CreatedAt.ToUniversalTime().Date;

		// Load existing daily balance for this tenant/account/date range.
		var start = dateUtc;
		var end = dateUtc.AddDays(1).AddTicks(-1);

		var existing = await _dailyBalanceRepository
			.GetByTenantAndPeriodAsync(tenantId, start, end)
			.ConfigureAwait(false);

		var currentBalance = existing.FirstOrDefault()?.Balance ?? 0m;

		var delta = @event.Type == TransactionType.Credit
	 ? @event.Amount
	 : -@event.Amount;

		var newBalance = currentBalance + delta;

		var dailyBalance = new DailyBalance(tenantId, accountId, dateUtc, newBalance);

		await _dailyBalanceRepository.UpsertAsync(dailyBalance).ConfigureAwait(false);
	}
}
