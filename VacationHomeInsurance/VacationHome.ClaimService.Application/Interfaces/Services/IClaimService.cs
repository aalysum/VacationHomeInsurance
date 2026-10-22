using VacationHome.ClaimService.Application.Common;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.Interfaces.Services
{
    public interface IClaimService
    {
        Task<ClaimDetailDto> CreateAsync(
            CreateClaimRequest request,
            CancellationToken cancellationToken = default);

        Task<ClaimCoverageEligibilityDto> GetEligibleCoveragesAsync(
            Guid policyId,
            ClaimType incidentType,
            CancellationToken cancellationToken = default);


        Task<ClaimDetailDto> GetAsync(
            Guid claimId,
            CancellationToken cancellationToken = default);


        Task<PagedResult<ClaimSummaryDto>> SearchAsync(
            ClaimSearchRequest request,
            CancellationToken cancellationToken = default);


        Task UpdateAsync(
            Guid claimId,
            UpdateClaimRequest request,
            CancellationToken cancellationToken = default);


        Task WithdrawAsync(
            Guid claimId,
            WithdrawClaimRequest request,
            CancellationToken cancellationToken = default);
    }
}
