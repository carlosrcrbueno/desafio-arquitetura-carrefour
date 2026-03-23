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
            .GetByAccountAndPeriodAsync(request.AccountId, request.StartDate, request.EndDate)
            .ConfigureAwait(false);

        return balances
            .Select(b => new DailyBalanceDto
            {
                AccountId = b.AccountId,
                Date = b.Date,
                Balance = b.Balance
            })
            .ToList();
    }
}
