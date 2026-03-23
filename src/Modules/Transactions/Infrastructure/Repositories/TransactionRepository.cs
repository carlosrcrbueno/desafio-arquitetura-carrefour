namespace Transactions.Infrastructure.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Shared.Database;
using Transactions.Domain.Entities;
using Transactions.Domain.Enums;
using Transactions.Domain.Interfaces;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(Transaction transaction)
    {
        if (transaction is null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        var tableName = GetPartitionTableName(transaction.CreatedAt);

        await EnsurePartitionTableExistsAsync(tableName).ConfigureAwait(false);

        const string insertSqlTemplate = @"INSERT INTO {0} (Id, AccountId, Amount, Type, CreatedAt) VALUES (@Id, @AccountId, @Amount, @Type, @CreatedAt);";
        var insertSql = string.Format(insertSqlTemplate, tableName);

        // Use synchronous open since IDbConnection does not expose OpenAsync.
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        await using var command = (SqlCommand)connection.CreateCommand();
        command.CommandText = insertSql;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = transaction.Id });
        command.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.UniqueIdentifier) { Value = transaction.AccountId });
        command.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal) { Value = transaction.Amount });
        command.Parameters.Add(new SqlParameter("@Type", SqlDbType.Int) { Value = (int)transaction.Type });
        command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = transaction.CreatedAt });

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Transaction>> GetByAccountAndPeriodAsync(
        Guid accountId,
        DateTime startDate,
        DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(endDate));
        }

        var results = new List<Transaction>();

        foreach (var tableName in GetPartitionTableNamesInRange(startDate, endDate))
        {
            var selectSql = $"SELECT Id, AccountId, Amount, Type, CreatedAt FROM {tableName} WHERE AccountId = @AccountId AND CreatedAt >= @StartDate AND CreatedAt <= @EndDate";

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            await using var command = (SqlCommand)connection.CreateCommand();
            command.CommandText = selectSql;

            command.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.UniqueIdentifier) { Value = accountId });
            command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = startDate });
            command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = endDate });

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var id = reader.GetGuid(0);
                var accountIdValue = reader.GetGuid(1);
                var amount = reader.GetDecimal(2);
                var type = (TransactionType)reader.GetInt32(3);
                var createdAt = reader.GetDateTime(4);

                var transaction = new Transaction(id, accountIdValue, amount, type, createdAt);
                results.Add(transaction);
            }
        }

        return results;
    }

    public async Task<IReadOnlyList<Transaction>> GetAllAsync()
    {
        var results = new List<Transaction>();

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        const string listTablesSql = "SELECT name FROM sys.tables WHERE name LIKE 'transactions[_]%'";

        await using (var listCommand = (SqlCommand)connection.CreateCommand())
        {
            listCommand.CommandText = listTablesSql;

            await using var tablesReader = await listCommand.ExecuteReaderAsync().ConfigureAwait(false);
            var tableNames = new List<string>();
            while (await tablesReader.ReadAsync().ConfigureAwait(false))
            {
                tableNames.Add(tablesReader.GetString(0));
            }

            foreach (var tableName in tableNames)
            {
                var selectSql = $"SELECT Id, AccountId, Amount, Type, CreatedAt FROM {tableName}";

                await using var selectCommand = (SqlCommand)connection.CreateCommand();
                selectCommand.CommandText = selectSql;

                await using var reader = await selectCommand.ExecuteReaderAsync().ConfigureAwait(false);
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var id = reader.GetGuid(0);
                    var accountId = reader.GetGuid(1);
                    var amount = reader.GetDecimal(2);
                    var type = (TransactionType)reader.GetInt32(3);
                    var createdAt = reader.GetDateTime(4);

                    var transaction = new Transaction(id, accountId, amount, type, createdAt);
                    results.Add(transaction);
                }
            }
        }

        return results;
    }

    private static string GetPartitionTableName(DateTime createdAt)
    {
        return $"transactions_{createdAt:yyyy_MM}";
    }

    private async Task EnsurePartitionTableExistsAsync(string tableName)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        const string existsSql = "SELECT 1 FROM sys.tables WHERE name = @TableName";

        await using (var existsCommand = (SqlCommand)connection.CreateCommand())
        {
            existsCommand.CommandText = existsSql;
            existsCommand.Parameters.Add(new SqlParameter("@TableName", SqlDbType.NVarChar, 128) { Value = tableName });

            var existsResult = await existsCommand.ExecuteScalarAsync().ConfigureAwait(false);
            if (existsResult is not null)
            {
                return;
            }
        }

        var createSql = $@"CREATE TABLE {tableName} (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Type INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);";

        await using var createCommand = (SqlCommand)connection.CreateCommand();
        createCommand.CommandText = createSql;
        await createCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static IEnumerable<string> GetPartitionTableNamesInRange(DateTime startDate, DateTime endDate)
    {
        var current = new DateTime(startDate.Year, startDate.Month, 1);
        var end = new DateTime(endDate.Year, endDate.Month, 1);

        while (current <= end)
        {
            yield return GetPartitionTableName(current);
            current = current.AddMonths(1);
        }
    }
}
