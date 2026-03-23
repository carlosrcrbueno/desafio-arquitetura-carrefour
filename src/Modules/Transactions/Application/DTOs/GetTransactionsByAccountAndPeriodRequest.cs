namespace Transactions.Application.DTOs;

using System;

public class GetTransactionsByAccountAndPeriodRequest
{
    public Guid AccountId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
