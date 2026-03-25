namespace Balance.Domain.Entities;

using System;

public class DailyBalance
{
    public int TenantId { get; }
    public Guid AccountId { get; }
    public DateTime Date { get; }
    public decimal Balance { get; }

    public DailyBalance(int tenantId, Guid accountId, DateTime date, decimal balance)
    {
        TenantId = tenantId;
        AccountId = accountId;
        Date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        Balance = balance;
    }
}
