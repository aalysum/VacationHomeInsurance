using Microsoft.EntityFrameworkCore;
using VacationHome.Auth.Application.Interfaces.Repositories;
using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Infrastructure.Persistence.Context;


namespace VacationHome.Auth.Infrastructure.Persistence.Repositories
{
    public sealed class RefreshTokenRepository
        : IRefreshTokenRepository
    {
        private readonly AuthDbContext _dbContext;


        public RefreshTokenRepository(
            AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task AddAsync(
            RefreshToken entity,
            CancellationToken cancellationToken)
        {
            await _dbContext.RefreshTokens.AddAsync(
                entity,
                cancellationToken);
        }


        public async Task<RefreshToken?> GetByHashAsync(
            string tokenHash,
            CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(
                    x => x.TokenHash == tokenHash,
                    cancellationToken);
        }


        public Task UpdateAsync(
            RefreshToken entity,
            CancellationToken cancellationToken)
        {
            _dbContext.RefreshTokens.Update(entity);
            return Task.CompletedTask;
        }


        public async Task RevokeAllActiveByUserIdAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            var revokedAtUtc =
                DateTime.UtcNow;


            await _dbContext.RefreshTokens
                .Where(x =>
                    x.UserId == userId &&
                    x.RevokedAtUtc == null &&
                    x.ExpiresAtUtc >
                        revokedAtUtc)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.RevokedAtUtc,
                            revokedAtUtc),
                    cancellationToken);
        }


        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(
                cancellationToken);
        }
    }
}
