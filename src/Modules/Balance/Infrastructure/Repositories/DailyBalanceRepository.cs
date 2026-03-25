namespace Balance.Infrastructure.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Balance.Domain.Entities;
using Balance.Domain.Interfaces;
using Shared.Database;

public class DailyBalanceRepository : IDailyBalanceRepository
{
	private readonly IDbConnectionFactory _connectionFactory;

	public DailyBalanceRepository(IDbConnectionFactory connectionFactory)
	{
		_connectionFactory = connectionFactory;
	}

	public Task UpsertAsync(DailyBalance balance)
	{
		if (balance is null)
		{
			throw new ArgumentNullException(nameof(balance));
		}

     const string sql = @"INSERT INTO DailyBalances (TenantId, Date, Balance)
VALUES (@TenantId, @Date, @Balance)
ON CONFLICT (TenantId, Date)
DO UPDATE SET Balance = EXCLUDED.Balance;";

		using var connection = _connectionFactory.CreateConnection();
		connection.Open();

		// Ensure the DailyBalances table exists before attempting to upsert on this connection.
		EnsureTableExists(connection);

		using var command = connection.CreateCommand();
		command.CommandText = sql;

     AddParameter(command, "@TenantId", DbType.Int32, balance.TenantId);
		AddParameter(command, "@Date", DbType.DateTime, balance.Date.ToUniversalTime());
		AddParameter(command, "@Balance", DbType.Int64, balance.BalanceInCents);

		command.ExecuteNonQuery();

		return Task.CompletedTask;
	}

	public Task<IReadOnlyList<DailyBalance>> GetByTenantAndPeriodAsync(
			   int tenantId,
		   DateTime startDate,
		   DateTime endDate)
	{
		try
		{
			if (endDate < startDate)
			{
				throw new ArgumentException("End date must be greater than or equal to start date.", nameof(endDate));
			}

          const string sql = @"SELECT TenantId, Date, Balance
FROM DailyBalances
WHERE TenantId = @TenantId AND Date >= @StartDate AND Date <= @EndDate
ORDER BY Date";

			var results = new List<DailyBalance>();

			using var connection = _connectionFactory.CreateConnection();
			connection.Open();

			// Ensure the DailyBalances table exists before querying on this connection.
			EnsureTableExists(connection);

			using var command = connection.CreateCommand();
			command.CommandText = sql;

			AddParameter(command, "@TenantId", DbType.Int32, tenantId);
			AddParameter(command, "@StartDate", DbType.DateTime, startDate.ToUniversalTime());
			AddParameter(command, "@EndDate", DbType.DateTime, endDate.ToUniversalTime());

			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
             var tenantIdValue = reader.GetInt32(0);
				var dateTime = reader.GetDateTime(1).ToUniversalTime();
				var balanceInCents = reader.GetInt64(2);
				// Usa Guid.Empty como AccountId pois o modelo atual não diferencia contas neste contexto
				var dailyBalance = new DailyBalance(tenantIdValue, Guid.Empty, dateTime, balanceInCents);
				results.Add(dailyBalance);
			}

			return Task.FromResult<IReadOnlyList<DailyBalance>>(results);

		}
		catch (Exception ex)
		{

			throw;
		}

	}

	public async Task<DailyBalance?> GetByTenantAndDateAsync(int tenantId, DateTime dateUtc)
	{
		var day = DateTime.SpecifyKind(dateUtc.Date, DateTimeKind.Utc);
		var list = await GetByTenantAndPeriodAsync(tenantId, day, day).ConfigureAwait(false);
		return list.FirstOrDefault();
	}

	public Task DeleteAllAsync()
	{
		const string sql = "DELETE FROM DailyBalances";

		using var connection = _connectionFactory.CreateConnection();
		connection.Open();

		using var command = connection.CreateCommand();
		command.CommandText = sql;

		command.ExecuteNonQuery();

		return Task.CompletedTask;
	}

	private static void EnsureTableExists(IDbConnection connection)
	{
     const string createSql = @"
CREATE TABLE IF NOT EXISTS DailyBalances (
	TenantId integer NOT NULL,
	Date timestamp NOT NULL,
	Balance bigint NOT NULL,
	CONSTRAINT PK_DailyBalances PRIMARY KEY (TenantId, Date)
);";

		using var createCommand = connection.CreateCommand();
		createCommand.CommandText = createSql;
		createCommand.ExecuteNonQuery();
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
