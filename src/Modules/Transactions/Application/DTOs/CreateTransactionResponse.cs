namespace Transactions.Application.DTOs;

using System;

public class CreateTransactionResponse
{
    public Guid TransactionId { get; init; }
    public Guid AccountId { get; init; }
}
