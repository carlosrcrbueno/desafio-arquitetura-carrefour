namespace Balance.Tests.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Balance.Domain.Entities;
using Balance.Domain.Interfaces;

public class FakeDailyBalanceRepository : IDailyBalanceRepository
{
    private readonly List<DailyBalance> _balances = new();

    public Task UpsertAsync(DailyBalance balance)
    {
        if (balance is null)
        {
            throw new ArgumentNullException(nameof(balance));
        }

        var existing = _balances.FirstOrDefault(b => b.AccountId == balance.AccountId && b.Date == balance.Date);
        if (existing is not null)
        {
            _balances.Remove(existing);
        }

        _balances.Add(balance);
        return Task.CompletedTask;
    }

   public Task<IReadOnlyList<DailyBalance>> GetByTenantAndPeriodAsync(int tenantId, DateTime startDate, DateTime endDate)
    {
        var result = _balances
            .Where(b => b.TenantId == tenantId && b.Date >= startDate.ToUniversalTime() && b.Date <= endDate.ToUniversalTime())
            .OrderBy(b => b.Date)
            .ToList();

        return Task.FromResult<IReadOnlyList<DailyBalance>>(result);
    }

    public Task DeleteAllAsync()
    {
        _balances.Clear();
        return Task.CompletedTask;
    }

    public IReadOnlyList<DailyBalance> GetAll() => _balances.ToList();
}
