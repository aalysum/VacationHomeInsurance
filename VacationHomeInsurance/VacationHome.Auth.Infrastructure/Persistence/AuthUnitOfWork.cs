using Microsoft.EntityFrameworkCore;
using VacationHome.Auth.Application.Interfaces.Persistence;
using VacationHome.Auth.Infrastructure.Persistence.Context;


namespace VacationHome.Auth.Infrastructure.Persistence
{
    public sealed class AuthUnitOfWork
        : IAuthUnitOfWork
    {
        private readonly AuthDbContext _dbContext;


        public AuthUnitOfWork(
            AuthDbContext dbContext)
        {
            _dbContext =
                dbContext ??
                throw new ArgumentNullException(
                    nameof(dbContext));
        }


        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(operation);


            var executionStrategy =
                _dbContext.Database
                    .CreateExecutionStrategy();


            await executionStrategy.ExecuteAsync(
                async () =>
                {
                    await using var transaction =
                        await _dbContext.Database
                            .BeginTransactionAsync(
                                cancellationToken);


                    try
                    {
                        await operation(
                            cancellationToken);


                        await _dbContext
                            .SaveChangesAsync(
                                cancellationToken);


                        await transaction.CommitAsync(
                            cancellationToken);
                    }
                    catch
                    {
                        await transaction.RollbackAsync(
                            cancellationToken);


                        throw;
                    }
                });
        }


        public async Task<T>
            ExecuteInTransactionAsync<T>(
                Func<CancellationToken, Task<T>>
                    operation,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(operation);


            var executionStrategy =
                _dbContext.Database
                    .CreateExecutionStrategy();


            return await executionStrategy.ExecuteAsync(
                async () =>
                {
                    await using var transaction =
                        await _dbContext.Database
                            .BeginTransactionAsync(
                                cancellationToken);


                    try
                    {
                        var result =
                            await operation(
                                cancellationToken);


                        await _dbContext
                            .SaveChangesAsync(
                                cancellationToken);


                        await transaction.CommitAsync(
                            cancellationToken);


                        return result;
                    }
                    catch
                    {
                        await transaction.RollbackAsync(
                            cancellationToken);


                        throw;
                    }
                });
        }


        public async Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
    }
}
