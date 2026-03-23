namespace Transactions.Domain.Entities;

using System;
using Transactions.Domain.Enums;

public class Transaction
{
    public Guid Id { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public TransactionType Type { get; }
    public DateTime CreatedAt { get; }

    public Transaction(Guid id, Guid accountId, decimal amount, TransactionType type, DateTime createdAt)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        Id = id;
        AccountId = accountId;
        Amount = amount;
        Type = type;
        CreatedAt = createdAt;
    }
}
