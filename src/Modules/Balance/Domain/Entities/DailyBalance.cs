namespace Balance.Domain.Entities;

using System;

public class DailyBalance
{
    public Guid AccountId { get; }
    public DateOnly Date { get; }
    public decimal Balance { get; }

    public DailyBalance(Guid accountId, DateOnly date, decimal balance)
    {
        AccountId = accountId;
        Date = date;
        Balance = balance;
    }
}
