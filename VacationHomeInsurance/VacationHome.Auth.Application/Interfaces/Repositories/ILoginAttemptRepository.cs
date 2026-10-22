using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Application.Interfaces.Repositories
{


    public interface ILoginAttemptRepository
    {
        Task AddAsync(
            LoginAttempt entity,
            CancellationToken cancellationToken);


        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}
