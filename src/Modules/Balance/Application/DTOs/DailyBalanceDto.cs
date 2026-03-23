namespace Balance.Application.DTOs;

using System;

public class DailyBalanceDto
{
    public Guid AccountId { get; init; }
    public DateOnly Date { get; init; }
    public decimal Balance { get; init; }
}
