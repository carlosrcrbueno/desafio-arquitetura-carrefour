namespace Api.Controllers;

using System;
using System.Threading.Tasks;
using Balance.Application.DTOs;
using Balance.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

// TenantId é resolvido por middleware (ex.: AuthorizationMockMiddleware)
// e armazenado no HttpContext.Items["TenantId"].

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
    public async Task<IActionResult> GetDaily([FromQuery] DateTime startDate)
    {
        if (startDate == default)
        {
            return BadRequest(new { error = "Invalid query parameters" });
        }

        if (!HttpContext.Items.TryGetValue("TenantId", out var tenantObj) || tenantObj is not int tenantId || tenantId <= 0)
        {
            return BadRequest(new { error = "TenantId is required" });
        }

        var request = new GetDailyBalanceRequest
        {
            TenantId = tenantId,
            StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc)
        };

        var result = await _getDailyBalanceUseCase.ExecuteAsync(request).ConfigureAwait(false);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild()
    {
        await _rebuildDailyBalancesUseCase.ExecuteAsync().ConfigureAwait(false);
        return NoContent();
    }
}
