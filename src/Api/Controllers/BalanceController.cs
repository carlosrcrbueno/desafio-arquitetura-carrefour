namespace Api.Controllers;

using System;
using System.Threading.Tasks;
using Balance.Application.DTOs;
using Balance.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

[Route("balances")]
public class BalanceController : ControllerBase
{
	private readonly IGetDailyBalanceUseCase _getDailyBalanceUseCase;

	public BalanceController(IGetDailyBalanceUseCase getDailyBalanceUseCase)
	{
		_getDailyBalanceUseCase = getDailyBalanceUseCase;
	}

	[HttpGet("daily")]
	public async Task<IActionResult> GetDaily([FromQuery] DateTime startDate, [FromQuery] DateTime? endDate = null)
	{
		if (startDate == default)
		{
			return BadRequest(new { error = "startDate is required" });
		}

		var request = new GetDailyBalanceRequest
		{
			TenantId = 0, // será injetado pelo TenantInjectionFilter
			StartDate = startDate,
			EndDate = endDate ?? startDate
		};

		var result = await _getDailyBalanceUseCase.ExecuteAsync(request).ConfigureAwait(false);
		if (result is null)
		{
			return NotFound();
		}

		return Ok(result);
	}
}
