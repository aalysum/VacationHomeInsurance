using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VacationHome.ProductQuote.Application.Exceptions;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;


namespace VacationHome.ProductQuote.Infrastructure.Persistence
{
    public sealed class UnitOfWork
        : IUnitOfWork
    {
        private readonly ProductQuoteDbContext
            _dbContext;


        public UnitOfWork(
            ProductQuoteDbContext dbContext)
        {
            _dbContext =
                dbContext;
        }


        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                return await _dbContext
                    .SaveChangesAsync(
                        cancellationToken);
            }
            catch (
                DbUpdateConcurrencyException
                exception)
            {
                throw new ConcurrencyConflictException(
                    "The record was modified by another operation. Refresh the data and try again.",
                    exception);
            }
        }


        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                operation);


            /*
             * If the caller is already inside a transaction,
             * execute the operation using that transaction.
             */
            if (
                _dbContext.Database
                    .CurrentTransaction
                is not null)
            {
                await operation(
                    cancellationToken);


                return;
            }


            /*
             * EnableRetryOnFailure requires manually created
             * transactions to execute inside the configured
             * EF Core execution strategy.
             */
            IExecutionStrategy executionStrategy =
                _dbContext.Database
                    .CreateExecutionStrategy();


            await executionStrategy.ExecuteAsync(
                async () =>
                {
                    await using
                        IDbContextTransaction transaction =
                            await _dbContext.Database
                                .BeginTransactionAsync(
                                    cancellationToken);


                    try
                    {
                        await operation(
                            cancellationToken);


                        await transaction.CommitAsync(
                            cancellationToken);
                    }
                    catch
                    {
                        await RollbackSafelyAsync(
                            transaction);


                        throw;
                    }
                });
        }


        public async Task<T>
            ExecuteInTransactionAsync<T>(
                Func<
                    CancellationToken,
                    Task<T>>
                    operation,
                CancellationToken
                    cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                operation);


            /*
             * Reuse the current transaction when this method
             * is called from another transactional operation.
             */
            if (
                _dbContext.Database
                    .CurrentTransaction
                is not null)
            {
                return await operation(
                    cancellationToken);
            }


            IExecutionStrategy executionStrategy =
                _dbContext.Database
                    .CreateExecutionStrategy();


            return await executionStrategy
                .ExecuteAsync(
                    async () =>
                    {
                        await using
                            IDbContextTransaction transaction =
                                await _dbContext.Database
                                    .BeginTransactionAsync(
                                        cancellationToken);


                        try
                        {
                            T result =
                                await operation(
                                    cancellationToken);


                            await transaction.CommitAsync(
                                cancellationToken);


                            return result;
                        }
                        catch
                        {
                            await RollbackSafelyAsync(
                                transaction);


                            throw;
                        }
                    });
        }


        private static async Task
            RollbackSafelyAsync(
                IDbContextTransaction transaction)
        {
            try
            {
                /*
                 * Cancellation of the original request should
                 * not prevent the database rollback.
                 */
                await transaction.RollbackAsync(
                    CancellationToken.None);
            }
            catch
            {
                /*
                 * Preserve the original operation exception.
                 * Disposing the transaction will release its
                 * database resources.
                 */
            }
        }
    }
}
