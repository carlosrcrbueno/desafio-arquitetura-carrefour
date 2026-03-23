namespace Transactions.Application.DTOs;

using System;
using Transactions.Domain.Enums;

public class CreateTransactionRequest
{
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
}
