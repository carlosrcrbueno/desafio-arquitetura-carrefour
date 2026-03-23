namespace Transactions.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transactions.Domain.Entities;

public interface ITransactionRepository
{
    Task InsertAsync(Transaction transaction);

    Task<IReadOnlyList<Transaction>> GetByAccountAndPeriodAsync(
        Guid accountId,
        DateTime startDate,
        DateTime endDate
    );

    Task<IReadOnlyList<Transaction>> GetAllAsync();
}
