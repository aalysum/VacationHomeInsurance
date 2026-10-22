using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Application.Interfaces.Persistence
{
    public interface IClaimDocumentRepository
    {
        Task AddAsync(
            ClaimDocument document,
            CancellationToken cancellationToken = default);


        Task<ClaimDocument?> GetByIdAsync(
            Guid id,
            bool track = true,
            CancellationToken cancellationToken = default);


        Task<IReadOnlyCollection<ClaimDocument>> GetByClaimIdAsync(
            Guid claimId,
            CancellationToken cancellationToken = default);


        Task DeleteAsync(
            ClaimDocument document,
            CancellationToken cancellationToken = default);
    }
}
