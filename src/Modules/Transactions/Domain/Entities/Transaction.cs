namespace Transactions.Domain.Entities;

using System;
using Shared.Enums;

public class Transaction
{
 public int TenantId { get; }
    public Guid Id { get; }
    public Guid AccountId { get; }
  public long AmountInCents { get; }
    public decimal Amount => AmountInCents / 100m;
    public TransactionType Type { get; }
    public DateTime CreatedAt { get; }
    public string IdempotenceKey { get; }

    public Transaction(int tenantId, Guid id, Guid accountId, long amountInCents, TransactionType type, DateTime createdAt, string idempotenceKey)
    {
        if (amountInCents <= 0)
        {
         throw new ArgumentOutOfRangeException(nameof(amountInCents), "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(idempotenceKey))
        {
            throw new ArgumentException("Idempotence key must be provided.", nameof(idempotenceKey));
        }

        TenantId = tenantId;
        Id = id;
        AccountId = accountId;
        AmountInCents = amountInCents;
        Type = type;
        CreatedAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
       IdempotenceKey = idempotenceKey;
    }
}
