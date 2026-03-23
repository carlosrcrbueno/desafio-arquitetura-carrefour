namespace Balance.Application.UseCases;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Balance.Domain.Entities;
using Balance.Domain.Interfaces;
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

        var balances = new Dictionary<(Guid AccountId, DateOnly Date), decimal>();

        foreach (var transaction in ordered)
        {
            var key = (transaction.AccountId, DateOnly.FromDateTime(transaction.CreatedAt.Date));

            if (!balances.TryGetValue(key, out var current))
            {
                current = 0m;
            }

            var newBalance = transaction.Type == Transactions.Domain.Enums.TransactionType.Credit
                ? current + transaction.Amount
                : current - transaction.Amount;

            balances[key] = newBalance;
        }

        // Recreate DailyBalances from computed dictionary.
        await _dailyBalanceRepository.DeleteAllAsync().ConfigureAwait(false);

        foreach (var entry in balances)
        {
            var (accountId, date) = entry.Key;
            var balance = entry.Value;

            var dailyBalance = new DailyBalance(accountId, date, balance);
            await _dailyBalanceRepository.UpsertAsync(dailyBalance).ConfigureAwait(false);
        }
    }
}
