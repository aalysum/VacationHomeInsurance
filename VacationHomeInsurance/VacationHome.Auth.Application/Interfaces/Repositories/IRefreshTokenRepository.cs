using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(
            RefreshToken entity,
            CancellationToken cancellationToken);


        Task<RefreshToken?> GetByHashAsync(
            string tokenHash,
            CancellationToken cancellationToken);


        Task UpdateAsync(
            RefreshToken entity,
            CancellationToken cancellationToken);


        Task RevokeAllActiveByUserIdAsync(
            string userId,
            CancellationToken cancellationToken);


        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}
