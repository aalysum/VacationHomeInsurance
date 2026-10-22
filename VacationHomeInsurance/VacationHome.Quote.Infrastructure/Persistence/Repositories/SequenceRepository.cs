using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Data.Common;
using VacationHome.ProductQuote.Application
    .Interfaces.Persistence;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Repositories
{
    public sealed class SequenceRepository :
        ISequenceRepository
    {
        private readonly ProductQuoteDbContext _dbContext;


        public SequenceRepository(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<long> GetNextQuoteSequenceAsync(
            int year,
            CancellationToken cancellationToken)
        {
            if (year < 2000 || year > 9999)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(year),
                    "Sequence year is invalid.");
            }


            IDbContextTransaction? ownedTransaction = null;


            try
            {
                if (_dbContext.Database.CurrentTransaction is null)
                {
                    ownedTransaction =
                        await _dbContext.Database
                            .BeginTransactionAsync(
                                IsolationLevel.Serializable,
                                cancellationToken);
                }


                DbConnection connection =
                    _dbContext.Database.GetDbConnection();


                if (connection.State !=
                    ConnectionState.Open)
                {
                    await connection.OpenAsync(
                        cancellationToken);
                }


                await using DbCommand command =
                    connection.CreateCommand();


                command.CommandText =
                    """
                    SET NOCOUNT ON;


                    DECLARE @GeneratedValue TABLE
                    (
                        [Value] BIGINT NOT NULL
                    );


                    UPDATE [QuoteNumberSequences]
                    WITH (UPDLOCK, HOLDLOCK)
                    SET
                        [LastValue] = [LastValue] + 1,
                        [UpdatedAtUtc] = SYSUTCDATETIME()
                    OUTPUT INSERTED.[LastValue]
                        INTO @GeneratedValue ([Value])
                    WHERE [Year] = @Year;


                    IF NOT EXISTS
                    (
                        SELECT 1
                        FROM @GeneratedValue
                    )
                    BEGIN
                        INSERT INTO [QuoteNumberSequences]
                        (
                            [Year],
                            [LastValue],
                            [UpdatedAtUtc]
                        )
                        VALUES
                        (
                            @Year,
                            1,
                            SYSUTCDATETIME()
                        );


                        INSERT INTO @GeneratedValue ([Value])
                        VALUES (1);
                    END;


                    SELECT TOP (1) [Value]
                    FROM @GeneratedValue;
                    """;


                DbParameter yearParameter =
                    command.CreateParameter();


                yearParameter.ParameterName = "@Year";
                yearParameter.DbType = DbType.Int32;
                yearParameter.Value = year;


                command.Parameters.Add(yearParameter);


                IDbContextTransaction? currentTransaction =
                    _dbContext.Database.CurrentTransaction;


                if (currentTransaction is not null)
                {
                    command.Transaction =
                        currentTransaction.GetDbTransaction();
                }


                object? result =
                    await command.ExecuteScalarAsync(
                        cancellationToken);


                if (result is null ||
                    result == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "The next Quote sequence value could not be generated.");
                }


                long sequenceValue =
                    Convert.ToInt64(result);


                if (ownedTransaction is not null)
                {
                    await ownedTransaction.CommitAsync(
                        cancellationToken);
                }


                return sequenceValue;
            }
            catch
            {
                if (ownedTransaction is not null)
                {
                    await ownedTransaction.RollbackAsync(
                        CancellationToken.None);
                }


                throw;
            }
            finally
            {
                if (ownedTransaction is not null)
                {
                    await ownedTransaction.DisposeAsync();
                }
            }
        }
    }
}
