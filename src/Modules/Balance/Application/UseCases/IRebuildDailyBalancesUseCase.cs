namespace Balance.Application.UseCases;

using System.Threading.Tasks;

public interface IRebuildDailyBalancesUseCase
{
    Task ExecuteAsync();
}
