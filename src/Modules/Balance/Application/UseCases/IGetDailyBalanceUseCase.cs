namespace Balance.Application.UseCases;

using System.Collections.Generic;
using System.Threading.Tasks;
using Balance.Application.DTOs;

public interface IGetDailyBalanceUseCase
{
    Task<IReadOnlyList<DailyBalanceDto>> ExecuteAsync(GetDailyBalanceRequest request);
}
