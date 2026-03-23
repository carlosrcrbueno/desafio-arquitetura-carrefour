namespace Api.Controllers;

using System;
using System.Threading.Tasks;
using Balance.Application.DTOs;
using Balance.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

[Route("balances")]
public class BalancesController : ControllerBase
{
    private readonly IGetDailyBalanceUseCase _getDailyBalanceUseCase;
    private readonly IRebuildDailyBalancesUseCase _rebuildDailyBalancesUseCase;

    public BalancesController(
        IGetDailyBalanceUseCase getDailyBalanceUseCase,
        IRebuildDailyBalancesUseCase rebuildDailyBalancesUseCase)
    {
        _getDailyBalanceUseCase = getDailyBalanceUseCase;
        _rebuildDailyBalancesUseCase = rebuildDailyBalancesUseCase;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily(
        [FromQuery] Guid accountId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (accountId == Guid.Empty || startDate == default || endDate == default || endDate < startDate)
        {
            return BadRequest(new { error = "Invalid query parameters" });
        }

        var request = new GetDailyBalanceRequest
        {
            AccountId = accountId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _getDailyBalanceUseCase.ExecuteAsync(request).ConfigureAwait(false);

        return Ok(result);
    }

    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild()
    {
        await _rebuildDailyBalancesUseCase.ExecuteAsync().ConfigureAwait(false);
        return NoContent();
    }
}
