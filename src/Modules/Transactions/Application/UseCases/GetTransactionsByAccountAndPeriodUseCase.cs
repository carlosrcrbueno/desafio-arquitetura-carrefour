namespace Transactions.Application.UseCases;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Application.DTOs;
using Transactions.Domain.Interfaces;

public class GetTransactionsByAccountAndPeriodUseCase : IGetTransactionsByAccountAndPeriodUseCase
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsByAccountAndPeriodUseCase(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<IReadOnlyList<TransactionDto>> ExecuteAsync(GetTransactionsByAccountAndPeriodRequest request)
    {
        var transactions = await _transactionRepository
            .GetByAccountAndPeriodAsync(request.AccountId, request.StartDate, request.EndDate)
            .ConfigureAwait(false);

        return transactions
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                Amount = t.Amount,
                Type = t.Type,
                CreatedAt = t.CreatedAt
            })
            .ToList();
    }
}
