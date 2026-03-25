namespace Balance.Application.UseCases;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Balance.Domain.Entities;
using Balance.Domain.Interfaces;
using Shared.Enums;
using Transactions.Domain.Entities;
using Transactions.Domain.Interfaces;

public class RebuildDailyBalancesUseCase : IRebuildDailyBalancesUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IDailyBalanceRepository _dailyBalanceRepository;

    public RebuildDailyBalancesUseCase(
        ITransactionRepository transactionRepository,
        IDailyBalanceRepository dailyBalanceRepository)
    {
        _transactionRepository = transactionRepository;
        _dailyBalanceRepository = dailyBalanceRepository;
    }

    public async Task ExecuteAsync()
    {
        // Read all transactions from all partitions, ordered by CreatedAt.
        var transactions = await _transactionRepository.GetAllAsync().ConfigureAwait(false);
        var ordered = transactions
            .OrderBy(t => t.AccountId)
            .ThenBy(t => t.CreatedAt)
            .ToList();

        // Key: TenantId + AccountId + Date (daily granularity, UTC)
        var balances = new Dictionary<(int TenantId, Guid AccountId, DateTime Date), decimal>();

        foreach (var transaction in ordered)
        {
            var date = transaction.CreatedAt.ToUniversalTime().Date;
            var key = (transaction.TenantId, transaction.AccountId, date);

            if (!balances.TryGetValue(key, out var current))
            {
                current = 0m;
            }

            var delta = transaction.Type == TransactionType.Credit
                ? transaction.Amount
                : -transaction.Amount;

            balances[key] = current + delta;
        }

        // Recreate DailyBalances from computed dictionary.
        await _dailyBalanceRepository.DeleteAllAsync().ConfigureAwait(false);

        foreach (var entry in balances)
        {
            var (tenantId, accountId, date) = entry.Key;
            var balance = entry.Value;

            var dailyBalance = new DailyBalance(tenantId, accountId, date, balance);
            await _dailyBalanceRepository.UpsertAsync(dailyBalance).ConfigureAwait(false);
        }
    }
}
