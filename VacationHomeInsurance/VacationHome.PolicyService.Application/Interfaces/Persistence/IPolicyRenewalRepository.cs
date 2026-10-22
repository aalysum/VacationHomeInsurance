using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Application.Interfaces.Persistence
{
    public interface IPolicyRenewalRepository
    {
        Task AddAsync(
            PolicyRenewal renewal,
            CancellationToken cancellationToken);


        Task<PolicyRenewal?> GetActiveAsync(
            Guid policyId,
            CancellationToken cancellationToken);


        Task<PolicyRenewal?> GetByIdAsync(
            Guid renewalId,
            bool track = false,
            CancellationToken cancellationToken = default);


        Task SaveChangesAsync(
            CancellationToken cancellationToken);
        Task<PolicyRenewal?> GetByRenewalQuoteIdAsync(
            Guid quoteId,
            bool track,
            CancellationToken cancellationToken);
    }
}
