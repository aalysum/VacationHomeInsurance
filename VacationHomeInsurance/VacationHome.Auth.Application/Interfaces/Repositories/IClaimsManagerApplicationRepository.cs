using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Application.Interfaces.Repositories
{


    public interface IClaimsManagerApplicationRepository
    {
        Task AddAsync(
            ClaimsManagerApplication entity,
            CancellationToken cancellationToken);


        Task<ClaimsManagerApplication?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken);
        Task<ClaimsManagerApplication?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken);
        Task<List<ClaimsManagerApplication>>GetPendingAsync(
                CancellationToken cancellationToken);
        Task UpdateAsync(
            ClaimsManagerApplication entity,
            CancellationToken cancellationToken);
        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}
