using VacationHome.ClaimService.Application.DTOs.External;
using VacationHome.ClaimService.Application.DTOs.Documents;


namespace VacationHome.ClaimService.Application
    .Interfaces.External
{
    public interface IPolicyServiceClient
    {
        Task<PolicyDetailsDto?> GetPolicyAsync(
            Guid policyId,
            CancellationToken cancellationToken = default);

        Task<PolicyDocumentContentDto?> DownloadPolicyDocumentAsync(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken = default);
    }
}
