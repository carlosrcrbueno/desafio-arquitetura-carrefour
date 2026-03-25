namespace Balance.Application.UseCases;

using System.Threading.Tasks;
using Balance.Application.DTOs;

public interface IGetDailyBalanceUseCase
{
  Task<DailyBalanceDto?> ExecuteAsync(GetDailyBalanceRequest request);
}
