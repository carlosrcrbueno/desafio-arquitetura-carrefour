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
		var dateUtc = @event.CreatedAt.ToUniversalTime().Date;

		// Load existing daily balance for this tenant/account/date range.
		var start = dateUtc;
		var end = dateUtc.AddDays(1).AddTicks(-1);

        var existing = await _dailyBalanceRepository
			.GetByTenantAndPeriodAsync(tenantId, start, end)
			.ConfigureAwait(false);

		var currentBalanceInCents = existing.FirstOrDefault()?.BalanceInCents ?? 0L;

		var deltaInCents = @event.Type == TransactionType.Credit
			? (long)Math.Round(@event.Amount * 100m, MidpointRounding.AwayFromZero)
			: -(long)Math.Round(@event.Amount * 100m, MidpointRounding.AwayFromZero);

		var newBalanceInCents = currentBalanceInCents + deltaInCents;

		var dailyBalance = new DailyBalance(tenantId, Guid.Empty, dateUtc, newBalanceInCents);

		await _dailyBalanceRepository.UpsertAsync(dailyBalance).ConfigureAwait(false);
	}
}
