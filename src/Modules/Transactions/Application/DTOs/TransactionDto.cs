namespace Transactions.Application.DTOs;

using System;
using Shared.Enums;

public class TransactionDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime CreatedAt { get; init; }
}
