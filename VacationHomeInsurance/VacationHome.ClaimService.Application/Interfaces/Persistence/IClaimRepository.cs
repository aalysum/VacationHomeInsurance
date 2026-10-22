using VacationHome.ClaimService.Application.Common;
using VacationHome.ClaimService.Domain.Entities;
using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application
    .Interfaces.Persistence
{
    public interface IClaimRepository
    {
        Task AddAsync(
            Claim claim,
            CancellationToken cancellationToken = default);


        Task UpdateAsync(
            Claim claim,
            CancellationToken cancellationToken = default);

        void AddStatusHistory(
            ClaimStatusHistory history);


        Task<Claim?> GetByIdAsync(
            Guid claimId,
            bool track = true,
            CancellationToken cancellationToken = default);


        Task<Claim?> GetByNumberAsync(
            string claimNumber,
            bool track = true,
            CancellationToken cancellationToken = default);


        Task<PagedResult<Claim>> SearchAsync(
            Guid customerId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);


        Task<IReadOnlyCollection<Claim>>
            GetOpenClaimsByPolicyAsync(
                Guid policyId,
                CancellationToken cancellationToken = default);


        Task<decimal> GetCommittedAmountAsync(
            Guid policyId,
            Guid coverageId,
            Guid? excludingClaimId,
            CancellationToken cancellationToken = default);


        void SetOriginalRowVersion(
            Claim claim,
            string rowVersion);


        Task<PagedResult<Claim>> SearchAdminAsync(
        ClaimStatus? status,
        ClaimType? claimType,
        Guid? customerId,
        string? claimNumber,
        DateTime? fromDate,
        DateTime? toDate,
        string? assignedClaimsManagerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    }
}
