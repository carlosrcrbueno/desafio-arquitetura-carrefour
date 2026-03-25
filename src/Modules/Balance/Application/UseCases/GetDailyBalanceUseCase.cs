namespace Balance.Application.UseCases;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Balance.Application.DTOs;
using Balance.Domain.Interfaces;

public class GetDailyBalanceUseCase : IGetDailyBalanceUseCase
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;

    public GetDailyBalanceUseCase(IDailyBalanceRepository dailyBalanceRepository)
    {
        _dailyBalanceRepository = dailyBalanceRepository;
    }

    public async Task<IReadOnlyList<DailyBalanceDto>> ExecuteAsync(GetDailyBalanceRequest request)
    {
        var balances = await _dailyBalanceRepository
            .GetByTenantAndPeriodAsync(request.TenantId, request.StartDate, request.EndDate)
            .ConfigureAwait(false);

        return balances
            .Select(b => new DailyBalanceDto
            {
                TenantId = b.TenantId,
                AccountId = b.AccountId,
                Date = b.Date,
                Balance = b.Balance
            })
            .ToList();
    }
}
