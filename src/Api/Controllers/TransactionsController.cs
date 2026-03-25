namespace Api.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Transactions.Application.DTOs;
using Transactions.Application.UseCases;

[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ICreateTransactionUseCase _createTransactionUseCase;
    private readonly IGetTransactionsByAccountAndPeriodUseCase _getTransactionsUseCase;

    public TransactionsController(
        ICreateTransactionUseCase createTransactionUseCase,
        IGetTransactionsByAccountAndPeriodUseCase getTransactionsUseCase)
    {
        _createTransactionUseCase = createTransactionUseCase;
        _getTransactionsUseCase = getTransactionsUseCase;
    }

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

        if (request is null)
        {
            return BadRequest(new { error = "Request is null" });
        }

        // TenantId must come from middleware (AuthorizationMockMiddleware) via HttpContext.Items.
        if (!HttpContext.Items.TryGetValue("TenantId", out var tenantObj) || tenantObj is not int tenantId || tenantId <= 0)
        {
            return BadRequest(new { error = "TenantId is required" });
        }

        request.TenantId = tenantId;

        var hasIdempotenceKey = !string.IsNullOrWhiteSpace(request.IdempotenceKey);
        var response = await _createTransactionUseCase.ExecuteAsync(request).ConfigureAwait(false);

        if (hasIdempotenceKey && !response.IsNew)
        {
            return Ok(new { message = "valor já processado" });
        }

        return StatusCode(201, new
        {
            transactionId = response.TransactionId,
            accountId = response.AccountId
        });
	}

	[HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] Guid accountId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (accountId == Guid.Empty || startDate == default || endDate == default || endDate < startDate)
        {
            return BadRequest(new { error = "Invalid query parameters" });
        }

        var request = new GetTransactionsByAccountAndPeriodRequest
        {
            AccountId = accountId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _getTransactionsUseCase.ExecuteAsync(request).ConfigureAwait(false);

        return Ok(result);
    }
}
