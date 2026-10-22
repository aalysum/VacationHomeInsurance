using VacationHome.PolicyService.Application.DTOs.Renewals;


namespace VacationHome.PolicyService.Application.Interfaces.Services
{
    public interface IRenewalService
    {
        Task<RenewalEligibilityDto> CheckEligibilityAsync(
            Guid policyId,
            Guid customerId,
            CancellationToken cancellationToken);


        Task<RenewalResultDto> CreateRenewalAsync(
            Guid policyId,
            Guid customerId,
            CancellationToken cancellationToken);


        Task CompleteRenewalAsync(
            Guid renewalQuoteId,
            Guid renewedPolicyId,
            CancellationToken cancellationToken);
    }
}
