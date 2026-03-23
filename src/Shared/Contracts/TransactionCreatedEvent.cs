namespace Shared.Contracts;

using System;
using Shared.Enums;

public class TransactionCreatedEvent
{
    public Guid TransactionId { get; init; }
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime CreatedAt { get; init; }
}
