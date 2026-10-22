using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Application.Interfaces.Persistence
{
    public interface IPolicyDocumentRepository
    {
        Task<PolicyDocument?> GetByIdAsync(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken);

        Task<PolicyDocument?> GetLatestAsync(
            Guid policyId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<PolicyDocument>> GetAllAsync(
            Guid policyId,
            CancellationToken cancellationToken);

        Task AddAsync(
            PolicyDocument document,
            CancellationToken cancellationToken);

        Task UpdateAsync(
            PolicyDocument document,
            CancellationToken cancellationToken);

        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}