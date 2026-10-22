using VacationHome.ClaimService.Application.Common;
using VacationHome.ClaimService.Application.DTOs.Admin;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Application.DTOs.Documents;
using VacationHome.ClaimService.Application.DTOs.External;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.External;
using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Application.Interfaces.Services;
using VacationHome.ClaimService.Application.Mappers;
using VacationHome.ClaimService.Domain.Entities;


using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.Services
{
    public sealed class ClaimAdminService
        : IClaimAdminService
    {
        private const int DashboardPageSize =
            100;
        private const string AdminRole =
    "Admin";


        private const string ClaimsManagerRole =
            "ClaimsManager";


        private readonly IClaimRepository
            _claimRepository;


        private readonly IAuthServiceClient
            _authServiceClient;

        private readonly IPolicyServiceClient
            _policyServiceClient;


        private readonly ICustomerServiceClient
            _customerServiceClient;


        private readonly ICurrentUserService
            _currentUserService;


        private readonly IAuditService
            _auditService;


        private readonly IUnitOfWork
            _unitOfWork;




        public ClaimAdminService(
            IClaimRepository claimRepository,
            IAuthServiceClient authServiceClient,
            IPolicyServiceClient policyServiceClient,
            ICustomerServiceClient customerServiceClient,
            ICurrentUserService currentUserService,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _claimRepository =
                claimRepository;


            _authServiceClient =
                authServiceClient;

            _policyServiceClient =
                policyServiceClient;


            _customerServiceClient =
                customerServiceClient;


            _currentUserService =
                currentUserService;


            _auditService =
                auditService;


            _unitOfWork =
                unitOfWork;
        }


        public async Task<ClaimDashboardDto>
            GetDashboardAsync(
                CancellationToken cancellationToken =
                    default)
        {
            ValidateReadAccess();


            string? assignedClaimsManagerId =
                GetClaimsManagerFilter();


            List<Claim> claims =
                await LoadAllClaimsForDashboardAsync(
                    assignedClaimsManagerId,
                    cancellationToken);


            return new ClaimDashboardDto
            {
                TotalClaims =
                    claims.Count,


                SubmittedClaims =
                    claims.LongCount(
                        claim =>
                            claim.Status ==
                            ClaimStatus.Submitted),


                UnderReviewClaims =
                    claims.LongCount(
                        claim =>
                            claim.Status ==
                            ClaimStatus.UnderReview),


                InformationRequestedClaims =
                    claims.LongCount(
                        claim =>
                            claim.Status ==
                            ClaimStatus
                                .AdditionalInformationRequested),


                ApprovedClaims =
                    claims.LongCount(
                        claim =>
                            claim.Status ==
                            ClaimStatus.Approved),


                RejectedClaims =
                    claims.LongCount(
                        claim =>
                            claim.Status ==
                            ClaimStatus.Rejected),


                PaidClaims =
                    claims.LongCount(
                        claim =>
                            claim.Status ==
                            ClaimStatus.Paid),


                ClosedClaims =
                    claims.LongCount(
                        claim =>
                            claim.Status ==
                            ClaimStatus.Closed),


                TotalClaimedAmount =
                    claims.Sum(
                        claim =>
                            claim.ClaimedAmount),


                TotalApprovedAmount =
                    claims.Sum(
                        claim =>
                            claim.ApprovedAmount ??
                            0m),


                RecentClaims =
                    claims
                        .OrderByDescending(
                            claim =>
                                claim.SubmittedAtUtc)
                        .Take(10)
                        .Select(
                            ClaimMapper.ToSummaryDto)
                        .ToArray()
            };
        }


        public async Task<PagedResult<ClaimDto>>
         GetClaimsAsync(
             ClaimDashboardFilterRequest request,
             CancellationToken cancellationToken =
                 default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateReadAccess();


            ValidateFilterRequest(
                request);


            string? assignedClaimsManagerId =
                GetClaimsManagerFilter();


            PagedResult<Claim> result =
                await _claimRepository
                    .SearchAdminAsync(
                        status:
                            request.Status,


                        claimType:
                            request.ClaimType,


                        customerId:
                            request.CustomerId,


                        claimNumber:
                            request.ClaimNumber,


                        fromDate:
                            request.FromDate,


                        toDate:
                            request.ToDate,


                        assignedClaimsManagerId:
                            assignedClaimsManagerId,


                        pageNumber:
                            request.PageNumber,


                        pageSize:
                            request.PageSize,


                        cancellationToken:
                            cancellationToken);


            return new PagedResult<ClaimDto>
            {
                PageNumber =
                    result.PageNumber,


                PageSize =
                    result.PageSize,


                TotalCount =
                    result.TotalCount,


                Items =
                    result.Items
                        .Select(
                            ClaimMapper.ToDto)
                        .ToArray()
            };
        }


        public async Task<ClaimDetailDto>
            GetClaimAsync(
                Guid claimId,
                CancellationToken cancellationToken =
                    default)
        {
            ValidateReadAccess();


            if (claimId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Claim identifier is required.");
            }


            Claim claim =
                await _claimRepository.GetByIdAsync(
                    claimId,
                    track: false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim not found.");


            RequireClaimReadAccess(
                claim);

            Task<PolicyDetailsDto> policyTask =
                GetRelatedPolicyAsync(
                    claim,
                    cancellationToken);

            Task<CustomerSummaryDto?> customerTask =
                _customerServiceClient.GetCustomerAsync(
                    claim.CustomerId,
                    cancellationToken);

            await Task.WhenAll(
                policyTask,
                customerTask);

            PolicyDetailsDto policy =
                await policyTask;

            CustomerSummaryDto? customer =
                await customerTask;

            if (
                customer is not null &&
                customer.CustomerId != claim.CustomerId
            )
            {
                throw new InvalidOperationException(
                    "Customer Service returned an unexpected customer.");
            }

            InternalCustomerContactDto? contact =
                customer is not null &&
                !string.IsNullOrWhiteSpace(customer.AuthUserId)
                    ? await _authServiceClient.GetCustomerContactAsync(
                        customer.AuthUserId,
                        cancellationToken)
                    : null;

            ClaimDetailDto details =
                claim.ToDetailDto();

            details.Policy =
                new ClaimPolicyDetailsDto
                {
                    PolicyNumber = policy.PolicyNumber,
                    ProductName = policy.ProductName,
                    Status = policy.Status.ToString(),
                    EffectiveDateUtc = policy.EffectiveDateUtc,
                    ExpiryDateUtc = policy.ExpiryDateUtc,
                    AnnualPremium = policy.AnnualPremium,
                    Coverages = policy.Coverages,
                    Documents = policy.Documents
                };

            details.Property =
                new ClaimPropertyDetailsDto
                {
                    Name = policy.PropertyName,
                    Type = policy.PropertyType,
                    AddressLine1 = policy.PropertyAddressLine1,
                    AddressLine2 = policy.PropertyAddressLine2,
                    City = policy.PropertyCity,
                    State = policy.PropertyState,
                    PostalCode = policy.PropertyPostalCode,
                    Country = policy.PropertyCountry,
                    PropertyValue = policy.PropertyValue,
                    ConstructionYear = policy.ConstructionYear,
                    OccupancyFrequency = policy.OccupancyFrequency,
                    HasSecuritySystem = policy.HasSecuritySystem
                };

            if (customer is not null)
            {
                details.Customer =
                    new ClaimCustomerDetailsDto
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Email = contact?.Email ?? string.Empty,
                        PhoneNumber = customer.PhoneNumber
                    };
            }

            return details;
        }

        public async Task<PolicyDocumentContentDto>
            DownloadPolicyDocumentAsync(
                Guid claimId,
                Guid documentId,
                CancellationToken cancellationToken = default)
        {
            ValidateReadAccess();

            if (claimId == Guid.Empty || documentId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Claim and policy document identifiers are required.");
            }

            Claim claim =
                await _claimRepository.GetByIdAsync(
                    claimId,
                    track: false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim not found.");

            RequireClaimReadAccess(claim);

            PolicyDetailsDto policy =
                await GetRelatedPolicyAsync(
                    claim,
                    cancellationToken);

            PolicyDocumentMetadataDto? document =
                policy.Documents.SingleOrDefault(
                    item => item.Id == documentId);

            if (
                document is null ||
                !string.Equals(
                    document.Status,
                    "Available",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                throw new NotFoundException(
                    "The policy document is not available.");
            }

            return await _policyServiceClient.DownloadPolicyDocumentAsync(
                policy.Id,
                documentId,
                cancellationToken)
                ?? throw new NotFoundException(
                    "The policy document file was not found.");
        }

        private async Task<PolicyDetailsDto> GetRelatedPolicyAsync(
            Claim claim,
            CancellationToken cancellationToken)
        {
            PolicyDetailsDto policy =
                await _policyServiceClient.GetPolicyAsync(
                    claim.PolicyId,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "The policy related to this claim was not found.");

            if (
                policy.CustomerId != claim.CustomerId ||
                policy.PropertyId != claim.PropertyId ||
                (!string.IsNullOrWhiteSpace(claim.PolicyNumber) &&
                    !string.Equals(
                        policy.PolicyNumber,
                        claim.PolicyNumber,
                        StringComparison.Ordinal))
            )
            {
                throw new InvalidOperationException(
                    "Policy Service returned policy details that do not match the claim.");
            }

            return policy;
        }

        private void ValidateReadAccess()
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
                    "Authenticated user identifier is missing.");
            }


            bool isAdmin =
                string.Equals(
                    _currentUserService.Role,
                    AdminRole,
                    StringComparison.OrdinalIgnoreCase);


            bool isClaimsManager =
                string.Equals(
                    _currentUserService.Role,
                    ClaimsManagerRole,
                    StringComparison.OrdinalIgnoreCase);


            if (
                !isAdmin &&
                !isClaimsManager
            )
            {
                throw new ForbiddenException(
                    "Admin or Claims Manager access is required.");
            }
        }


        private void RequireClaimReadAccess(
            Claim claim)
        {
            ArgumentNullException.ThrowIfNull(
                claim);


            bool isAdmin =
                string.Equals(
                    _currentUserService.Role,
                    AdminRole,
                    StringComparison.OrdinalIgnoreCase);


            if (isAdmin)
            {
                return;
            }


            bool isClaimsManager =
                string.Equals(
                    _currentUserService.Role,
                    ClaimsManagerRole,
                    StringComparison.OrdinalIgnoreCase);


            if (!isClaimsManager)
            {
                throw new ForbiddenException(
                    "Claim access is not permitted.");
            }


            string authUserId =
                _currentUserService
                    .AuthUserId
                    .Trim();


            if (
                string.IsNullOrWhiteSpace(
                    claim.AssignedClaimsManagerId) ||
                !string.Equals(
                    claim.AssignedClaimsManagerId,
                    authUserId,
                    StringComparison.Ordinal)
            )
            {
                throw new ForbiddenException(
                    "Only the assigned Claims Manager can view this Claim.");
            }
        }




        private string?  GetClaimsManagerFilter()
        {
            bool isClaimsManager =
                string.Equals(
                    _currentUserService.Role,
                    ClaimsManagerRole,
                    StringComparison.OrdinalIgnoreCase);


            if (!isClaimsManager)
            {
                return null;
            }


            string authUserId =
                _currentUserService
                    .AuthUserId
                    .Trim();


            if (
                string.IsNullOrWhiteSpace(
                    authUserId)
            )
            {
                throw new UnauthorizedException(
                    "Authenticated Claims Manager identifier is missing.");
            }


            return authUserId;
        }


        public async Task<ClaimDetailDto>
            AssignClaimsManagerAsync(
                Guid claimId,
                AssignClaimsManagerRequest request,
                CancellationToken cancellationToken =
                    default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateAdminUser();


            if (claimId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Claim identifier is required.");
            }


            string claimsManagerId =
                NormalizeRequiredText(
                    request.ClaimsManagerId,
                    "Claims Manager identifier is required.");


            Claim claim =
                await _claimRepository.GetByIdAsync(
                    claimId,
                    track: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim not found.");


            ValidateAssignmentStatus(
                claim);


            Console.WriteLine(
                $"Incoming RowVersion: {request.RowVersion}");


            Console.WriteLine(
                $"Entity RowVersion: {Convert.ToBase64String(claim.RowVersion)}");


            _claimRepository.SetOriginalRowVersion(
                claim,
                request.RowVersion);


            if (
                string.Equals(
                    claim.AssignedClaimsManagerId,
                    claimsManagerId,
                    StringComparison.Ordinal)
            )
            {
                throw new BadRequestException(
                    "The Claim is already assigned to the selected Claims Manager.");
            }


            bool isReassignment =
                !string.IsNullOrWhiteSpace(
                    claim.AssignedClaimsManagerId);


            string? reason =
                NormalizeOptionalText(
                    request.Reason);


            if (
                isReassignment &&
                string.IsNullOrWhiteSpace(
        reason)
)
            {
                throw new BadRequestException(
                    "A reason is required when reassigning a Claim.");
            }


            if (
                reason is not null &&
                reason.Length > 2000
            )
            {
                throw new BadRequestException(
                    "Assignment reason cannot exceed 2000 characters.");
            }


            ActiveClaimsManagerDto?
                claimsManager =
                    await _authServiceClient
                        .GetActiveClaimsManagerAsync(
                            claimsManagerId,
                            cancellationToken);


            if (claimsManager is null)
            {
                throw new BadRequestException(
                    "The selected user is not an active, approved Claims Manager.");
            }


            string returnedClaimsManagerId =
                NormalizeRequiredText(
                    claimsManager.UserId,
                    "Auth Service returned an invalid Claims Manager identifier.");


            if (
                !string.Equals(
                    returnedClaimsManagerId,
                    claimsManagerId,
                    StringComparison.Ordinal)
            )
            {
                throw new BadRequestException(
                    "Auth Service returned an unexpected Claims Manager.");
            }


            string claimsManagerName =
                ResolveClaimsManagerName(
                    claimsManager);


            string claimsManagerEmail =
                NormalizeRequiredText(
                    claimsManager.Email,
                    "The selected Claims Manager does not have a valid email address.");


            DateTime assignedAtUtc =
                DateTime.UtcNow;


            string assignedBy =
                GetAdminActor();


            string? previousClaimsManagerId =
                claim.AssignedClaimsManagerId;


            string? previousClaimsManagerName =
                claim.AssignedClaimsManagerName;


            string? previousClaimsManagerEmail =
                claim.AssignedClaimsManagerEmail;


            claim.AssignedClaimsManagerId =
                returnedClaimsManagerId;


            claim.AssignedClaimsManagerName =
                claimsManagerName;


            claim.AssignedClaimsManagerEmail =
                claimsManagerEmail;


            claim.AssignedAtUtc =
                assignedAtUtc;


            claim.AssignedBy =
                assignedBy;


            claim.LastModifiedAtUtc =
                assignedAtUtc;


            claim.LastModifiedBy =
                assignedBy;


            // TEMPORARILY REMOVE ASSIGNMENT HISTORY


            /*
            var history = new ClaimAssignmentHistory
            {
                Id = Guid.NewGuid(),
                ClaimId = claim.Id,


                PreviousClaimsManagerId =
                    NormalizeOptionalText(
                        previousClaimsManagerId),


                PreviousClaimsManagerName =
                    NormalizeOptionalText(
                        previousClaimsManagerName),


                PreviousClaimsManagerEmail =
                    NormalizeOptionalText(
                        previousClaimsManagerEmail),


                ClaimsManagerId =
                    returnedClaimsManagerId,


                ClaimsManagerName =
                    claimsManagerName,


                ClaimsManagerEmail =
                    claimsManagerEmail,


                Reason =
                    reason,


                AssignedBy =
                    assignedBy,


                AssignedAtUtc =
                    assignedAtUtc
            };


            claim.AssignmentHistory.Add(history);
            */


            await _claimRepository.UpdateAsync(
                claim,
                cancellationToken);


            string auditAction =
                isReassignment
                    ? "CLAIM_REASSIGNED"
                    : "CLAIM_ASSIGNED";


            string auditDetails =
                isReassignment
                    ? BuildReassignmentAuditDetails(
                        previousClaimsManagerId,
                        returnedClaimsManagerId,
                        reason)
                    : BuildAssignmentAuditDetails(
                        returnedClaimsManagerId);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                auditAction,
                auditDetails,
                cancellationToken);


            try
            {
                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);
                Console.WriteLine(
    $"Before Save RowVersion: {Convert.ToBase64String(claim.RowVersion)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    ex.ToString());


                throw;
            }


            return claim.ToDetailDto();
        }


        private async Task<List<Claim>>
            LoadAllClaimsForDashboardAsync(
                string? assignedClaimsManagerId,
                CancellationToken cancellationToken)
        {
            var claims =
                new List<Claim>();


            int pageNumber =
                1;


            while (true)
            {
                cancellationToken
                    .ThrowIfCancellationRequested();


                PagedResult<Claim> page =
                    await _claimRepository
                        .SearchAdminAsync(
                            status: null,
                            claimType: null,
                            customerId: null,
                            claimNumber: null,
                            fromDate: null,
                            toDate: null,
                            assignedClaimsManagerId:
                                assignedClaimsManagerId,
                            pageNumber:
                                pageNumber,
                            pageSize:
                                DashboardPageSize,
                            cancellationToken:
                                cancellationToken);


                claims.AddRange(
                    page.Items);


                if (
                    page.Items.Count <
                    DashboardPageSize
                )
                {
                    break;
                }


                long loadedCount =
                    (long)pageNumber *
                    DashboardPageSize;


                if (
                    loadedCount >=
                    page.TotalCount
                )
                {
                    break;
                }


                pageNumber++;
            }


            return claims;
        }
        private void ValidateAdminUser()
        {
            if (
                !_currentUserService
                    .IsAuthenticated ||
                string.IsNullOrWhiteSpace(
                    _currentUserService.AuthUserId)
            )
            {
                throw new UnauthorizedException(
                    "Authenticated Admin identifier is missing.");
            }


            if (
                !string.Equals(
                    _currentUserService.Role,
                    AdminRole,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                throw new ForbiddenException(
                    "Only an Admin can assign or reassign a Claims Manager.");
            }
        }


        private string GetAdminActor()
        {
            if (
                !string.IsNullOrWhiteSpace(
                    _currentUserService.Email)
            )
            {
                return _currentUserService.Email;
            }


            if (
                string.IsNullOrWhiteSpace(
                    _currentUserService.AuthUserId)
            )
            {
                throw new UnauthorizedException(
                    "Authenticated Admin identity is missing.");
            }


            return _currentUserService.AuthUserId;
        }


        private static void ValidateAssignmentStatus(
            Claim claim)
        {
            if (
                claim.Status ==
                    ClaimStatus.Approved ||
                claim.Status ==
                    ClaimStatus.Rejected ||
                claim.Status ==
                    ClaimStatus.Paid ||
                claim.Status ==
                    ClaimStatus.Closed ||
                claim.Status ==
                    ClaimStatus.Withdrawn
            )
            {
                throw new BadRequestException(
                    "A Claims Manager cannot be assigned or reassigned after the Claim has reached a final decision or payment state.");
            }
        }


        private static void ValidateFilterRequest(
            ClaimDashboardFilterRequest request)
        {
            if (request.PageNumber < 1)
            {
                throw new BadRequestException(
                    "Page number must be greater than zero.");
            }


            if (
                request.PageSize < 1 ||
                request.PageSize > 100
            )
            {
                throw new BadRequestException(
                    "Page size must be between 1 and 100.");
            }


            if (
                request.FromDate.HasValue &&
                request.ToDate.HasValue &&
                request.FromDate.Value >
                    request.ToDate.Value
            )
            {
                throw new BadRequestException(
                    "From-date must not be later than to-date.");
            }


            if (
                !string.IsNullOrWhiteSpace(
                    request.ClaimNumber) &&
                request.ClaimNumber
                    .Trim()
                    .Length > 100
            )
            {
                throw new BadRequestException(
                    "Claim number filter cannot exceed 100 characters.");
            }
        }


        private static string
            ResolveClaimsManagerName(
                ActiveClaimsManagerDto claimsManager)
        {
            string name =
                string.Join(
                    " ",
                    new[]
                    {
                claimsManager.FirstName,
                claimsManager.LastName
                    }
                    .Where(
                        value =>
                            !string.IsNullOrWhiteSpace(
                                value))
                    .Select(
                        value =>
                            value.Trim()));


            if (
                string.IsNullOrWhiteSpace(
                    name) &&
                !string.IsNullOrWhiteSpace(
                    claimsManager.DisplayName)
            )
            {
                name =
                    claimsManager.DisplayName
                        .Trim();
            }


            if (
                string.IsNullOrWhiteSpace(
                    name)
            )
            {
                throw new BadRequestException(
                    "The selected Claims Manager does not have a valid display name.");
            }


            return name;
        }


        private static string
            NormalizeRequiredText(
                string? value,
                string errorMessage)
        {
            if (
                string.IsNullOrWhiteSpace(
                    value)
            )
            {
                throw new BadRequestException(
                    errorMessage);
            }


            return value.Trim();
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


        private static string
            BuildAssignmentAuditDetails(
                string claimsManagerId)
        {
            return
                $"Claims Manager {claimsManagerId} " +
                "was assigned to the Claim.";
        }


        private static string
            BuildReassignmentAuditDetails(
                string? previousClaimsManagerId,
                string claimsManagerId,
                string? reason)
        {
            return
                $"Claim reassigned from " +
                $"{previousClaimsManagerId ?? "unassigned"} " +
                $"to {claimsManagerId}. " +
                $"Reason: {reason}.";
        }
    }
}
