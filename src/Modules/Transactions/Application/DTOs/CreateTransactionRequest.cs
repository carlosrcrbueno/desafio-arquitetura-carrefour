namespace Transactions.Application.DTOs;

using System;
using Shared.Enums;

public class CreateTransactionRequest
{
    public int TenantId { get; set; }
	public Guid AccountId => Guid.NewGuid();
	public decimal Amount { get; set; }
	public TransactionType Type { get; set; }
   public string? IdempotenceKey { get; set; }
}
