using VacationHome.PolicyService.Application.DTOs.Documents;


namespace VacationHome.PolicyService.Application.Interfaces.Services
{
    public interface IPolicyDocumentService
    {
        Task<PolicyDocumentDto> EnsureLatestGeneratedAsync(
            Guid policyId,
            CancellationToken cancellationToken);

        Task<PolicyDocumentDto> RegenerateForAdminAsync(
            Guid policyId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<PolicyDocumentDto>>
            GetForCustomerAsync(
                Guid policyId,
                Guid customerId,
                CancellationToken cancellationToken);

        Task<IReadOnlyList<PolicyDocumentDto>>
            GetForAdminAsync(
                Guid policyId,
                CancellationToken cancellationToken);

        Task<PolicyDocumentDownload?> DownloadForCustomerAsync(
            Guid policyId,
            Guid documentId,
            Guid customerId,
            CancellationToken cancellationToken);

        Task<PolicyDocumentDownload?> DownloadForAdminAsync(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken);

        Task<PolicyDocumentDownload?> DownloadForInternalServiceAsync(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken);
    }
}