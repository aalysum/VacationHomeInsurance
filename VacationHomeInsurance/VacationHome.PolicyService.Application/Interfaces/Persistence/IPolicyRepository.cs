using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.DTOs.Policies;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.Interfaces.Persistence
{
    public interface IPolicyRepository
    {
        Task AddAsync(
            Policy policy,
            CancellationToken cancellationToken);


        Task UpdateAsync(
            Policy policy,
            CancellationToken cancellationToken);


        Task<Policy?> GetByIdAsync(
            Guid policyId,
            bool track = false,
            CancellationToken cancellationToken = default);


        Task<Policy?> GetByQuoteIdAsync(
            Guid quoteId,
            bool track = false,
            CancellationToken cancellationToken = default);


        Task<IReadOnlyList<Policy>> GetCustomerPoliciesAsync(
            Guid customerId,
            CancellationToken cancellationToken);


        Task<PagedResult<Policy>> GetAdminPoliciesAsync(
            PolicyFilterRequest request,
            CancellationToken cancellationToken);


        Task<IReadOnlyList<Policy>> GetPoliciesForExpiryAsync(
            DateTime utcNow,
            CancellationToken cancellationToken);


        Task SaveChangesAsync(
            CancellationToken cancellationToken);
        Task<List<Policy>> GetActivePoliciesAsync(
            Guid customerId,
            Guid propertyId,
            Guid? excludedQuoteId,
            CancellationToken cancellationToken);


    }
}
