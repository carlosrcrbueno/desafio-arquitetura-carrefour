namespace Balance.Application.UseCases;

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

  public async Task<DailyBalanceDto?> ExecuteAsync(GetDailyBalanceRequest request)
    {
        var balance = await _dailyBalanceRepository
            .GetByTenantAndDateAsync(request.TenantId, request.StartDate)
            .ConfigureAwait(false);

        if (balance is null)
        {
            return null;
        }

        return new DailyBalanceDto
        {
            TenantId = balance.TenantId,
            AccountId = balance.AccountId,
            Date = balance.Date,
            Balance = balance.Balance
        };
    }
}
