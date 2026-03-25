namespace Balance.Application.DTOs;

using System;

public class GetDailyBalanceRequest
{
 public int TenantId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
