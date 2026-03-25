namespace Balance.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Balance.Domain.Entities;

public interface IDailyBalanceRepository
{
    Task UpsertAsync(DailyBalance balance);

    Task<IReadOnlyList<DailyBalance>> GetByTenantAndPeriodAsync(
        int tenantId,
        DateTime startDate,
        DateTime endDate
    );

    Task<DailyBalance?> GetByTenantAndDateAsync(int tenantId, DateTime dateUtc);

    Task DeleteAllAsync();
}
