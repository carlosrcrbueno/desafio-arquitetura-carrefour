namespace Balance.Application.DTOs;

using System;

public class GetDailyBalanceRequest
{
    public Guid AccountId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
