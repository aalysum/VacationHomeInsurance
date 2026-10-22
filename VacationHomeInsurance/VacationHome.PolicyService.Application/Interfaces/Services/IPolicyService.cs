using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.DTOs.Internal;
using VacationHome.PolicyService.Application.DTOs.Policies;


namespace VacationHome.PolicyService.Application
    .Interfaces.Services
{
    public interface IPolicyService
    {
        Task<PolicyDetailsDto> GetAsync(
            Guid policyId,
            Guid customerId,
            bool isAdmin,
            CancellationToken cancellationToken);


        Task<IReadOnlyCollection<PolicySummaryDto>>
            GetCustomerPoliciesAsync(
                Guid customerId,
                CancellationToken cancellationToken);


        Task<PagedResult<PolicySummaryDto>>
            GetAdminPoliciesAsync(
                PolicyFilterRequest request,
                CancellationToken cancellationToken);


        Task CancelAsync(
            Guid policyId,
            Guid customerId,
            CancelPolicyRequest request,
            CancellationToken cancellationToken);


        Task ExpirePoliciesAsync(
            CancellationToken cancellationToken);


        Task<PolicyOverlapResultDto>
            CheckOverlapAsync(
                PolicyOverlapRequestDto request,
                CancellationToken cancellationToken);


        Task<CoverageValidationResultDto>
            ValidateClaimCoverageAsync(
                Guid policyId,
                DateTime incidentDate,
                CancellationToken cancellationToken);


        Task<CoverageValidationResultDto>
            ValidateClaimAmountAsync(
                Guid policyId,
                decimal claimAmount,
                CancellationToken cancellationToken);
    }
}
