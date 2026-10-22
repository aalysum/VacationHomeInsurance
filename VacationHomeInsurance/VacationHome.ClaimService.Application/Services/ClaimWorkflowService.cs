using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Application.Interfaces.Services;
using VacationHome.ClaimService.Domain.Entities;
using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.Services
{
    public sealed class ClaimWorkflowService
        : IClaimWorkflowService
    {
        private const string ClaimsManagerRole =
            "ClaimsManager";


        private readonly IClaimRepository
            _claimRepository;


        private readonly IAuditService
            _auditService;


        private readonly ICurrentUserService
            _currentUserService;


        private readonly IUnitOfWork
            _unitOfWork;


        public ClaimWorkflowService(
            IClaimRepository claimRepository,
            IAuditService auditService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _claimRepository =
                claimRepository;


            _auditService =
                auditService;


            _currentUserService =
                currentUserService;


            _unitOfWork =
                unitOfWork;
        }


        public async Task MoveToReviewAsync(
            Guid claimId,
            string rowVersion,
            CancellationToken cancellationToken = default)
        {
            ValidateCurrentUser();


            Claim claim =
                await GetClaimAsync(
                    claimId,
                    cancellationToken);


            RequireAssignedClaimsManager(
                claim);


            ApplyOriginalRowVersion(
                claim,
                rowVersion);


            if (
                claim.Status !=
                    ClaimStatus.Submitted &&
                claim.Status !=
                    ClaimStatus
                        .AdditionalInformationRequested
            )
            {
                throw new BadRequestException(
                    "Only submitted Claims or Claims awaiting additional information can move to review.");
            }


            if (claim.Coverages.Count == 0)
            {
                throw new BadRequestException(
                    "The Claim has no selected Policy Coverages.");
            }


            if (
                claim.Coverages.Any(
                    coverage =>
                        coverage.RequestedAmount <= 0)
            )
            {
                throw new BadRequestException(
                    "Every Claim Coverage must have a requested amount greater than zero.");
            }


            ClaimStatus previousStatus =
                claim.Status;


            DateTime utcNow =
                DateTime.UtcNow;


            claim.Status =
                ClaimStatus.UnderReview;


            claim.LastModifiedAtUtc =
                utcNow;


            claim.LastModifiedBy =  
                GetActor();


            AddHistory(
                claim,
                previousStatus,
                ClaimStatus.UnderReview,
                "Claim moved to under review.",
                utcNow);


            await _claimRepository.UpdateAsync(
                claim,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "CLAIM_UNDER_REVIEW",
                null,
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        public async Task RequestInformationAsync(
            Guid claimId,
            RequestInformationRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateCurrentUser();


            Claim claim =
                await GetClaimAsync(
                    claimId,
                    cancellationToken);


            RequireAssignedClaimsManager(
                claim);


            ApplyOriginalRowVersion(
                claim,
                request.RowVersion);


            if (
                claim.Status !=
                ClaimStatus.UnderReview
            )
            {
                throw new BadRequestException(
                    "Only a Claim under review can request additional information.");
            }


            string message =
                request.Message?.Trim() ??
                string.Empty;


            if (
                string.IsNullOrWhiteSpace(
                    message)
            )
            {
                throw new BadRequestException(
                    "The additional-information message is required.");
            }


            if (message.Length > 2000)
            {
                throw new BadRequestException(
                    "The additional-information message cannot exceed 2000 characters.");
            }


            ClaimStatus previousStatus =
                claim.Status;


            DateTime utcNow =
                DateTime.UtcNow;


            claim.Status =
                ClaimStatus
                    .AdditionalInformationRequested;


            claim.LastModifiedAtUtc =
                utcNow;


            claim.LastModifiedBy =
                GetActor();


            AddHistory(
                claim,
                previousStatus,
                ClaimStatus.AdditionalInformationRequested,
                message,
                utcNow);


            await _claimRepository.UpdateAsync(
                claim,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "INFORMATION_REQUESTED",
                message,
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        public async Task ApproveAsync(
            Guid claimId,
            ApproveClaimRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateCurrentUser();


            Claim claim =
                await GetClaimAsync(
                    claimId,
                    cancellationToken);


            RequireAssignedClaimsManager(
                claim);


            ApplyOriginalRowVersion(
                claim,
                request.RowVersion);


            if (
                claim.Status !=
                ClaimStatus.UnderReview
            )
            {
                throw new BadRequestException(
                    "Only Claims under review can be approved.");
            }


            ValidateApprovalRequest(
                claim,
                request);


            Dictionary<Guid, decimal>
                approvedAmounts =
                    request.Coverages
                        .ToDictionary(
                            coverage =>
                                coverage.CoverageId,
                            coverage =>
                                coverage.ApprovedAmount);


            decimal totalApprovedAmount =
                0m;


            foreach (
                ClaimCoverage claimCoverage
                in claim.Coverages)
            {
                decimal approvedAmount =
                    approvedAmounts[
                        claimCoverage.CoverageId];


                decimal committedAmount =
                    await _claimRepository
                        .GetCommittedAmountAsync(
                            claim.PolicyId,
                            claimCoverage.CoverageId,
                            excludingClaimId:
                                claim.Id,
                            cancellationToken);


                decimal remainingLimit =
                    claimCoverage
                        .CoverageLimitAmount -
                    committedAmount;


                if (remainingLimit < 0)
                {
                    remainingLimit = 0;
                }


                if (
                    approvedAmount >
                    claimCoverage.RequestedAmount
                )
                {
                    throw new BadRequestException(
                        $"The approved amount for Coverage " +
                        $"'{claimCoverage.CoverageName}' cannot " +
                        $"exceed its requested amount of " +
                        $"{claimCoverage.RequestedAmount:0.00}.");
                }


                if (
                    approvedAmount >
                    remainingLimit
                )
                {
                    throw new BadRequestException(
                        $"The approved amount for Coverage " +
                        $"'{claimCoverage.CoverageName}' exceeds " +
                        $"its remaining available limit of " +
                        $"{remainingLimit:0.00}.");
                }


                claimCoverage.ApprovedAmount =
                    approvedAmount;


                totalApprovedAmount +=
                    approvedAmount;
            }


            if (totalApprovedAmount <= 0)
            {
                throw new BadRequestException(
                    "The total approved amount must be greater than zero. Reject the Claim if no Coverage amount can be approved.");
            }


            if (
                totalApprovedAmount >
                claim.ClaimedAmount
            )
            {
                throw new BadRequestException(
                    "The total approved amount cannot exceed the total claimed amount.");
            }


            string? notes =
                NormalizeOptionalText(
                    request.Notes);


            if (
                notes is not null &&
                notes.Length > 2000
            )
            {
                throw new BadRequestException(
                    "Approval notes cannot exceed 2000 characters.");
            }


            ClaimStatus previousStatus =
                claim.Status;


            DateTime utcNow =
                DateTime.UtcNow;


            claim.Status =
                ClaimStatus.Approved;


            claim.ApprovedAmount =
                totalApprovedAmount;


            claim.ApprovedAtUtc =
                utcNow;


            claim.RejectedAtUtc =
                null;


            claim.RejectionReason =
                null;


            claim.InternalNotes =
                notes;


            claim.LastModifiedAtUtc =
                utcNow;


            claim.LastModifiedBy =
                GetActor();


            AddHistory(
                claim,
                previousStatus,
                ClaimStatus.Approved,
                notes ??
                    $"Claim approved for a total amount of {totalApprovedAmount:0.00}.",
                utcNow);


            await _claimRepository.UpdateAsync(
                claim,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "CLAIM_APPROVED",
                $"Approved amount: " +
                $"{totalApprovedAmount:0.00}; " +
                $"Coverage count: " +
                $"{claim.Coverages.Count}.",
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        public async Task RejectAsync(
            Guid claimId,
            RejectClaimRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateCurrentUser();


            Claim claim =
                await GetClaimAsync(
                    claimId,
                    cancellationToken);


            RequireAssignedClaimsManager(
                claim);


            ApplyOriginalRowVersion(
                claim,
                request.RowVersion);


            if (
                claim.Status !=
                ClaimStatus.UnderReview
            )
            {
                throw new BadRequestException(
                    "Only Claims under review can be rejected.");
            }


            string reason =
                request.Reason?.Trim() ??
                string.Empty;


            if (
                string.IsNullOrWhiteSpace(
                    reason)
            )
            {
                throw new BadRequestException(
                    "A rejection reason is required.");
            }


            if (reason.Length < 20)
            {
                throw new BadRequestException(
                    "The rejection reason must contain at least 20 characters.");
            }


            if (reason.Length > 2000)
            {
                throw new BadRequestException(
                    "The rejection reason cannot exceed 2000 characters.");
            }


            string? notes =
                NormalizeOptionalText(
                    request.Notes);


            if (
                notes is not null &&
                notes.Length > 2000
            )
            {
                throw new BadRequestException(
                    "Rejection notes cannot exceed 2000 characters.");
            }


            ClaimStatus previousStatus =
                claim.Status;


            DateTime utcNow =
                DateTime.UtcNow;


            claim.Status =
                ClaimStatus.Rejected;


            claim.ApprovedAmount =
                null;


            claim.ApprovedAtUtc =
                null;


            claim.RejectedAtUtc =
                utcNow;


            claim.RejectionReason =
                reason;


            claim.InternalNotes =
                notes;


            claim.LastModifiedAtUtc =
                utcNow;


            claim.LastModifiedBy =
                GetActor();


            foreach (
                ClaimCoverage coverage
                in claim.Coverages)
            {
                coverage.ApprovedAmount =
                    null;
            }


            AddHistory(
                claim,
                previousStatus,
                ClaimStatus.Rejected,
                reason,
                utcNow);


            await _claimRepository.UpdateAsync(
                claim,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "CLAIM_REJECTED",
                reason,
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        public async Task MarkPaidAsync(
            Guid claimId,
            MarkClaimPaidRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateCurrentUser();


            Claim claim =
                await GetClaimAsync(
                    claimId,
                    cancellationToken);


            RequireAssignedClaimsManager(
                claim);


            ApplyOriginalRowVersion(
                claim,
                request.RowVersion);


            if (
                claim.Status !=
                ClaimStatus.Approved
            )
            {
                throw new BadRequestException(
                    "Only approved Claims can be marked as paid.");
            }


            if (
                !claim.ApprovedAmount.HasValue ||
                claim.ApprovedAmount.Value <= 0
            )
            {
                throw new BadRequestException(
                    "The Claim does not contain a valid approved amount.");
            }


            if (
                claim.Coverages.Count == 0 ||
                claim.Coverages.All(
                    coverage =>
                        !coverage.ApprovedAmount
                            .HasValue ||
                        coverage.ApprovedAmount
                            .Value <= 0)
            )
            {
                throw new BadRequestException(
                    "The Claim does not contain any approved Coverage amounts.");
            }


            decimal coverageApprovedTotal =
                claim.Coverages.Sum(
                    coverage =>
                        coverage.ApprovedAmount ??
                        0m);


            if (
                coverageApprovedTotal !=
                claim.ApprovedAmount.Value
            )
            {
                throw new BadRequestException(
                    "The Claim approved amount does not match the sum of its approved Coverage amounts.");
            }


            string paymentReference =
                request.PaymentReference?.Trim() ??
                string.Empty;


            if (
                string.IsNullOrWhiteSpace(
                    paymentReference)
            )
            {
                throw new BadRequestException(
                    "Payment reference is required.");
            }


            if (paymentReference.Length > 100)
            {
                throw new BadRequestException(
                    "Payment reference cannot exceed 100 characters.");
            }


            ClaimStatus previousStatus =
                claim.Status;


            DateTime utcNow =
                DateTime.UtcNow;


            claim.Status =
                ClaimStatus.Paid;


            claim.PaidAtUtc =
                utcNow;


            claim.LastModifiedAtUtc =
                utcNow;


            claim.LastModifiedBy =
                GetActor();


            AddHistory(
                claim,
                previousStatus,
                ClaimStatus.Paid,
                $"Payment reference: {paymentReference}",
                utcNow);


            await _claimRepository.UpdateAsync(
                claim,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "CLAIM_PAID",
                paymentReference,
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        public async Task CloseAsync(
            Guid claimId,
            string rowVersion,
            CancellationToken cancellationToken = default)
        {
            ValidateCurrentUser();


            Claim claim =
                await GetClaimAsync(
                    claimId,
                    cancellationToken);


            RequireAssignedClaimsManager(
                claim);


            ApplyOriginalRowVersion(
                claim,
                rowVersion);


            if (
                claim.Status !=
                    ClaimStatus.Paid &&
                claim.Status !=
                    ClaimStatus.Rejected
            )
            {
                throw new BadRequestException(
                    "Only paid or rejected Claims can be closed.");
            }


            ClaimStatus previousStatus =
                claim.Status;


            DateTime utcNow =
                DateTime.UtcNow;


            claim.Status =
                ClaimStatus.Closed;


            claim.ClosedAtUtc =
                utcNow;


            claim.LastModifiedAtUtc =
                utcNow;


            claim.LastModifiedBy =
                GetActor();


            AddHistory(
                claim,
                previousStatus,
                ClaimStatus.Closed,
                "Claim closed.",
                utcNow);


            await _claimRepository.UpdateAsync(
                claim,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "CLAIM_CLOSED",
                $"Previous status: {previousStatus}.",
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        private async Task<Claim>
            GetClaimAsync(
                Guid claimId,
                CancellationToken cancellationToken)
        {
            if (claimId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Claim identifier is required.");
            }


            return await _claimRepository
                .GetByIdAsync(
                    claimId,
                    track: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim was not found.");
        }


        private void RequireAssignedClaimsManager(
            Claim claim)
        {
            ArgumentNullException.ThrowIfNull(
                claim);


            if (
                string.IsNullOrWhiteSpace(
                    claim.AssignedClaimsManagerId)
            )
            {
                throw new ForbiddenException(
                    "The Claim has not been assigned to a Claims Manager.");
            }


            string authUserId =
                _currentUserService
                    .AuthUserId
                    ?.Trim() ??
                string.Empty;


            if (
                string.IsNullOrWhiteSpace(
                    authUserId)
            )
            {
                throw new UnauthorizedException(
                    "Authenticated Claims Manager identifier is missing.");
            }


            if (
                !string.Equals(
                    claim.AssignedClaimsManagerId.Trim(),
                    authUserId,
                    StringComparison.Ordinal)
            )
            {
                throw new ForbiddenException(
                    "Only the assigned Claims Manager can process this Claim.");
            }
        }


        private void ApplyOriginalRowVersion(
            Claim claim,
            string rowVersion)
        {
            if (
                string.IsNullOrWhiteSpace(
                    rowVersion)
            )
            {
                throw new BadRequestException(
                    "Claim RowVersion is required.");
            }


            _claimRepository
                .SetOriginalRowVersion(
                    claim,
                    rowVersion.Trim());
        }


        private static void ValidateApprovalRequest(
            Claim claim,
            ApproveClaimRequest request)
        {
            if (claim.Coverages.Count == 0)
            {
                throw new BadRequestException(
                    "The Claim has no Coverages to approve.");
            }


            if (
                request.Coverages is null ||
                request.Coverages.Count == 0
            )
            {
                throw new BadRequestException(
                    "Approval allocations are required for every Claim Coverage.");
            }


            bool containsInvalidCoverageId =
                request.Coverages.Any(
                    coverage =>
                        coverage.CoverageId ==
                        Guid.Empty);


            if (containsInvalidCoverageId)
            {
                throw new BadRequestException(
                    "Every approval allocation must contain a valid Coverage identifier.");
            }


            bool containsNegativeAmount =
                request.Coverages.Any(
                    coverage =>
                        coverage.ApprovedAmount < 0);


            if (containsNegativeAmount)
            {
                throw new BadRequestException(
                    "Approved Coverage amounts cannot be negative.");
            }


            bool containsDuplicateCoverage =
                request.Coverages
                    .GroupBy(
                        coverage =>
                            coverage.CoverageId)
                    .Any(
                        group =>
                            group.Count() > 1);


            if (containsDuplicateCoverage)
            {
                throw new BadRequestException(
                    "A Coverage cannot be included more than once in the approval request.");
            }


            HashSet<Guid> claimCoverageIds =
                claim.Coverages
                    .Select(
                        coverage =>
                            coverage.CoverageId)
                    .ToHashSet();


            HashSet<Guid> requestedCoverageIds =
                request.Coverages
                    .Select(
                        coverage =>
                            coverage.CoverageId)
                    .ToHashSet();


            if (
                !claimCoverageIds.SetEquals(
                    requestedCoverageIds)
            )
            {
                throw new BadRequestException(
                    "The approval request must contain exactly one allocation for every Claim Coverage.");
            }
        }


        private void AddHistory(
            Claim claim,
            ClaimStatus previousStatus,
            ClaimStatus newStatus,
            string comments,
            DateTime changedAtUtc)
        {
            _claimRepository.AddStatusHistory(
                new ClaimStatusHistory
                {
                    Id =
                        Guid.NewGuid(),


                    ClaimId =
                        claim.Id,


                    PreviousStatus =
                        previousStatus,


                    NewStatus =
                        newStatus,


                    Comments =
                        comments,


                    ChangedBy =
                        GetActor(),


                    ChangedAtUtc =
                        changedAtUtc
                });
        }


        private void ValidateCurrentUser()
        {
            if (
                !_currentUserService
                    .IsAuthenticated
            )
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
            }


            if (
                string.IsNullOrWhiteSpace(
                    _currentUserService.AuthUserId)
            )
            {
                throw new UnauthorizedException(
                    "The authenticated user identifier is missing.");
            }


            if (
                !string.Equals(
                    _currentUserService.Role,
                    ClaimsManagerRole,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                throw new ForbiddenException(
                    "Claims Manager access is required.");
            }


            if (
                string.IsNullOrWhiteSpace(
                    _currentUserService.Email)
            )
            {
                throw new UnauthorizedException(
                    "The authenticated user email is missing.");
            }
        }


        private string GetActor()
        {
            if (
                !string.IsNullOrWhiteSpace(
                    _currentUserService.Email)
            )
            {
                return _currentUserService
                    .Email
                    .Trim();
            }


            if (
                string.IsNullOrWhiteSpace(
                    _currentUserService.AuthUserId)
            )
            {
                throw new UnauthorizedException(
                    "Authenticated user identity is missing.");
            }


            return _currentUserService
                .AuthUserId
                .Trim();
        }


        private static string?
            NormalizeOptionalText(
                string? value)
        {
            if (
                string.IsNullOrWhiteSpace(
                    value)
            )
            {
                return null;
            }


            return value.Trim();
        }
    }
}
