namespace VacationHome.Auth.Application
    .Interfaces.Persistence
{
    public interface IAuthUnitOfWork
    {
        Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task>
                operation,
            CancellationToken cancellationToken);


        Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>>
                operation,
            CancellationToken cancellationToken);


        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}
