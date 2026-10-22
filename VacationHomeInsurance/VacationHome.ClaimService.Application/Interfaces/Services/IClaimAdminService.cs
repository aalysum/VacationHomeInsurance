using VacationHome.ClaimService.Application.Common;
using VacationHome.ClaimService.Application.DTOs.Admin;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Application.DTOs.Documents;


namespace VacationHome.ClaimService.Application
    .Interfaces.Services
{
    public interface IClaimAdminService
    {
        Task<ClaimDashboardDto>
            GetDashboardAsync(
                CancellationToken cancellationToken =
                    default);


        Task<PagedResult<ClaimDto>>
            GetClaimsAsync(
                ClaimDashboardFilterRequest request,
                CancellationToken cancellationToken =
                    default);


        Task<ClaimDetailDto>
            GetClaimAsync(
                Guid claimId,
                CancellationToken cancellationToken =
                    default);

        Task<PolicyDocumentContentDto>
            DownloadPolicyDocumentAsync(
                Guid claimId,
                Guid documentId,
                CancellationToken cancellationToken =
                    default);


        Task<ClaimDetailDto>
            AssignClaimsManagerAsync(
                Guid claimId,
                AssignClaimsManagerRequest request,
                CancellationToken cancellationToken =
                    default);
    }
}
