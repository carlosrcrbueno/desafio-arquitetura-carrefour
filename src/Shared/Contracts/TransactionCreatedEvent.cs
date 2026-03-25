namespace Shared.Contracts;

using System;
using Shared.Enums;

public class TransactionCreatedEvent
{
    public int TenantId { get; init; }
    public Guid TransactionId { get; init; }
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime CreatedAt { get; init; }
}
