using VacationHome.Auth.Application.Interfaces.Repositories;
using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Infrastructure.Persistence.Context;


namespace VacationHome.Auth.Infrastructure.Persistence.Repositories
{


    public class LoginAttemptRepository
        : ILoginAttemptRepository
    {
        private readonly AuthDbContext _dbContext;


        public LoginAttemptRepository(
            AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task AddAsync(
            LoginAttempt entity,
            CancellationToken cancellationToken)
        {
            await _dbContext.LoginAttempts
                .AddAsync(entity, cancellationToken);
        }


        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
