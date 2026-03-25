namespace Balance.Application.DTOs;

using System;

public class DailyBalanceDto
{
 public int TenantId { get; init; }
    public Guid AccountId { get; init; }
    public DateTime Date { get; init; }
    public decimal Balance { get; init; }
}
