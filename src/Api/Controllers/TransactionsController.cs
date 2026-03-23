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
        if (request is null)
        {
            return BadRequest(new { error = "Invalid request data" });
        }

        var response = await _createTransactionUseCase.ExecuteAsync(request).ConfigureAwait(false);

        return StatusCode(201, response);
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
