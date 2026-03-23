namespace Balance.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Balance.Domain.Entities;

public interface IDailyBalanceRepository
{
    Task UpsertAsync(DailyBalance balance);

    Task<IReadOnlyList<DailyBalance>> GetByAccountAndPeriodAsync(
        Guid accountId,
        DateTime startDate,
        DateTime endDate
    );

    Task DeleteAllAsync();
}
