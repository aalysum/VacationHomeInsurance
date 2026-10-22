using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Application.Interfaces.Repositories
{


    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(
            PasswordResetToken entity,
            CancellationToken cancellationToken);


        Task<PasswordResetToken?> GetByHashAndUserIdAsync(
            string hash,
            string userId,
            CancellationToken cancellationToken);
        Task UpdateAsync(
            PasswordResetToken entity,
            CancellationToken cancellationToken);


        Task SaveChangesAsync(
            CancellationToken cancellationToken);
        Task<List<PasswordResetToken>> GetActiveTokensByUserIdAsync(
            string userId,
            CancellationToken cancellationToken);
    }
}
