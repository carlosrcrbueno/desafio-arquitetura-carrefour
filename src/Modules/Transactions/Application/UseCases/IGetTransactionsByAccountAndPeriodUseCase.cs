namespace Transactions.Application.UseCases;

using System.Collections.Generic;
using System.Threading.Tasks;
using Transactions.Application.DTOs;

public interface IGetTransactionsByAccountAndPeriodUseCase
{
    Task<IReadOnlyList<TransactionDto>> ExecuteAsync(GetTransactionsByAccountAndPeriodRequest request);
}
