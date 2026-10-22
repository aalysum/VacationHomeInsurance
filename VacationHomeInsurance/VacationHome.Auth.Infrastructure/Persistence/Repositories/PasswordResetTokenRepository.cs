using Microsoft.EntityFrameworkCore;
using VacationHome.Auth.Application.Interfaces.Repositories;
using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Infrastructure.Persistence.Context;


namespace VacationHome.Auth.Infrastructure.Persistence.Repositories
{


    public class PasswordResetTokenRepository
        : IPasswordResetTokenRepository
    {
        private readonly AuthDbContext _dbContext;


        public PasswordResetTokenRepository(
            AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task AddAsync(
            PasswordResetToken entity,
            CancellationToken cancellationToken)
        {
            await _dbContext.PasswordResetTokens
                .AddAsync(entity, cancellationToken);
        }


        public async Task<PasswordResetToken?>
            GetByHashAndUserIdAsync(
                string hash,
                string userId,
                CancellationToken cancellationToken)
        {
            return await _dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(
                    x =>
                        x.TokenHash == hash &&
                        x.UserId == userId,
                    cancellationToken);
        }
        public Task UpdateAsync(
            PasswordResetToken entity,
            CancellationToken cancellationToken)
        {
            _dbContext.PasswordResetTokens.Update(entity);


            return Task.CompletedTask;
        }


        public async Task<List<PasswordResetToken>>
            GetActiveTokensByUserIdAsync(
                string userId,
                CancellationToken cancellationToken)
        {
            return await _dbContext.PasswordResetTokens
                .Where(x =>
                    x.UserId == userId &&
                    x.UsedAtUtc == null)
                .ToListAsync(cancellationToken);
        }


        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
