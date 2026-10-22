using VacationHome.ClaimService.Application.DTOs.Claims;


namespace VacationHome.ClaimService.Application.Interfaces.Services
{
    public interface IClaimWorkflowService
    {
        Task MoveToReviewAsync(
            Guid claimId,
            string rowVersion,
            CancellationToken cancellationToken = default);


        Task RequestInformationAsync(
            Guid claimId,
            RequestInformationRequest request,
            CancellationToken cancellationToken = default);


        Task ApproveAsync(
            Guid claimId,
            ApproveClaimRequest request,
            CancellationToken cancellationToken = default);


        Task RejectAsync(
            Guid claimId,
            RejectClaimRequest request,
            CancellationToken cancellationToken = default);


        Task MarkPaidAsync(
            Guid claimId,
            MarkClaimPaidRequest request,
            CancellationToken cancellationToken = default);


        Task CloseAsync(
            Guid claimId,
            string rowVersion,
            CancellationToken cancellationToken = default);
    }
}
