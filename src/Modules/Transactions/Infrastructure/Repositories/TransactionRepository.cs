namespace Transactions.Infrastructure.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Shared.Database;
using Shared.Enums;
using Transactions.Domain.Entities;
using Transactions.Domain.Interfaces;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task InsertAsync(Transaction transaction)
    {
        if (transaction is null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        var tableName = GetPartitionTableName(transaction.CreatedAt);

        EnsurePartitionTableExistsAsync(tableName).GetAwaiter().GetResult();

        const string insertSqlTemplate = @"INSERT INTO {0} (Id, AccountId, Amount, Type, CreatedAt) VALUES (@Id, @AccountId, @Amount, @Type, @CreatedAt);";
        var insertSql = string.Format(insertSqlTemplate, tableName);

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = insertSql;

        AddParameter(command, "@Id", DbType.Guid, transaction.Id);
        AddParameter(command, "@AccountId", DbType.Guid, transaction.AccountId);
        AddParameter(command, "@Amount", DbType.Decimal, transaction.Amount);
        AddParameter(command, "@Type", DbType.Int32, (int)transaction.Type);
        AddParameter(command, "@CreatedAt", DbType.DateTime, transaction.CreatedAt.ToUniversalTime());

        command.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Transaction>> GetByAccountAndPeriodAsync(
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

            using var command = connection.CreateCommand();
            command.CommandText = selectSql;

            AddParameter(command, "@AccountId", DbType.Guid, accountId);
            AddParameter(command, "@StartDate", DbType.DateTime, startDate.ToUniversalTime());
            AddParameter(command, "@EndDate", DbType.DateTime, endDate.ToUniversalTime());

            using var reader = command.ExecuteReader();
               while (reader.Read())
                {
                    var id = reader.GetGuid(0);
                    var accountIdValue = reader.GetGuid(1);
                    var amount = reader.GetDecimal(2);
                    var type = (TransactionType)reader.GetInt32(3);
                    var createdAt = reader.GetDateTime(4).ToUniversalTime();

                    var transaction = new Transaction(0, id, accountIdValue, amount, type, createdAt);
                    results.Add(transaction);
                }
        }

        return Task.FromResult<IReadOnlyList<Transaction>>(results);
    }

    public Task<IReadOnlyList<Transaction>> GetAllAsync()
    {
        var results = new List<Transaction>();

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        const string listTablesSql = "SELECT table_name FROM information_schema.tables WHERE table_name LIKE 'transactions_%'";

        using (var listCommand = connection.CreateCommand())
        {
            listCommand.CommandText = listTablesSql;

            using var tablesReader = listCommand.ExecuteReader();
            var tableNames = new List<string>();
            while (tablesReader.Read())
            {
                tableNames.Add(tablesReader.GetString(0));
            }

            foreach (var tableName in tableNames)
            {
                var selectSql = $"SELECT Id, AccountId, Amount, Type, CreatedAt FROM {tableName}";

                using var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = selectSql;

                using var reader = selectCommand.ExecuteReader();
               while (reader.Read())
                {
                    var id = reader.GetGuid(0);
                    var accountId = reader.GetGuid(1);
                    var amount = reader.GetDecimal(2);
                    var type = (TransactionType)reader.GetInt32(3);
                    var createdAt = reader.GetDateTime(4).ToUniversalTime();

                    var transaction = new Transaction(0, id, accountId, amount, type, createdAt);
                    results.Add(transaction);
                }
            }
        }

        return Task.FromResult<IReadOnlyList<Transaction>>(results);
    }

    private static string GetPartitionTableName(DateTime createdAt)
    {
        return $"transactions_{createdAt:yyyy_MM}";
    }

    private Task EnsurePartitionTableExistsAsync(string tableName)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        const string existsSql = "SELECT 1 FROM information_schema.tables WHERE table_name = @TableName";

        using (var existsCommand = connection.CreateCommand())
        {
            existsCommand.CommandText = existsSql;
            AddParameter(existsCommand, "@TableName", DbType.String, tableName);

            var existsResult = existsCommand.ExecuteScalar();
            if (existsResult is not null)
            {
                return Task.CompletedTask;
            }
        }

        var createSql = $@"CREATE TABLE {tableName} (
    Id uuid NOT NULL PRIMARY KEY,
    AccountId uuid NOT NULL,
    Amount numeric(18,2) NOT NULL,
    Type integer NOT NULL,
    CreatedAt timestamp NOT NULL
);";

        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = createSql;
        createCommand.ExecuteNonQuery();

        return Task.CompletedTask;
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

    private static void AddParameter(IDbCommand command, string name, DbType dbType, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = dbType;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
