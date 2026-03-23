namespace Transactions.Tests.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Domain.Entities;
using Transactions.Domain.Enums;
using Transactions.Domain.Interfaces;

public class FakeTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();
    public readonly List<string> UsedTables = new();

    public Task InsertAsync(Transaction transaction)
    {
        if (transaction is null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        var tableName = GetPartitionTableName(transaction.CreatedAt);
        UsedTables.Add(tableName);

        _transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Transaction>> GetByAccountAndPeriodAsync(Guid accountId, DateTime startDate, DateTime endDate)
    {
        var result = _transactions
            .Where(t => t.AccountId == accountId && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .OrderBy(t => t.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<Transaction>>(result);
    }

    public Task<IReadOnlyList<Transaction>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<Transaction>>(_transactions.ToList());
    }

    private static string GetPartitionTableName(DateTime createdAt)
    {
        return $"transactions_{createdAt:yyyy_MM}";
    }
}
