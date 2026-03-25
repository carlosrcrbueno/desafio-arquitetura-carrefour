namespace Transactions.Application.UseCases;

using System.Threading.Tasks;
using Transactions.Application.DTOs;

public interface ICreateTransactionUseCase
{
    Task<CreateTransactionResponse> ExecuteAsync(CreateTransactionRequest request);
}
