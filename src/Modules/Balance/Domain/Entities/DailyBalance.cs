namespace Balance.Domain.Entities;

using System;

public class DailyBalance
{
    public int TenantId { get; }
    public Guid AccountId { get; }
    public DateTime Date { get; }
 public long BalanceInCents { get; }
    public decimal Balance => BalanceInCents / 100m;

   public DailyBalance(int tenantId, Guid accountId, DateTime date, long balanceInCents)
    {
        TenantId = tenantId;
        AccountId = accountId;
        Date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
      BalanceInCents = balanceInCents;
    }
}
