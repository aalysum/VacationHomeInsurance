using VacationHome.ClaimService.Application.Common;
using VacationHome.ClaimService.Application.Configuration;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.External;
using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Application.Interfaces.Services;
using VacationHome.ClaimService.Application.Mappers;
using VacationHome.ClaimService.Domain.Entities;
using VacationHome.ClaimService.Domain.Enums;
using Microsoft.Extensions.Options;


namespace VacationHome.ClaimService.Application.Services
{
    public sealed class ClaimsService
        : IClaimService
    {
        private readonly IClaimRepository
            _claimRepository;


        private readonly ISequenceRepository
            _sequenceRepository;


        private readonly ICurrentUserService
            _currentUserService;


        private readonly IPolicyServiceClient
            _policyServiceClient;


        private readonly ICustomerServiceClient
            _customerServiceClient;


        private readonly IAuditService
            _auditService;


        private readonly IUnitOfWork
            _unitOfWork;
        private readonly ICustomerIdentityService
    _customerIdentityService;
        private readonly ClaimCoverageEligibilityOptions
            _coverageEligibilityOptions;


        public ClaimsService(
            IClaimRepository claimRepository,
            ISequenceRepository sequenceRepository,
            ICurrentUserService currentUserService,
            IPolicyServiceClient policyServiceClient,
            ICustomerServiceClient customerServiceClient,
            ICustomerIdentityService customerIdentityService,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            IOptions<ClaimCoverageEligibilityOptions>
                coverageEligibilityOptions)
        {
            _claimRepository =
                claimRepository;


            _sequenceRepository =
                sequenceRepository;


            _currentUserService =
                currentUserService;


            _policyServiceClient =
                policyServiceClient;


            _customerServiceClient =
                customerServiceClient;


            _customerIdentityService = customerIdentityService;


            _auditService =
                auditService;


            _unitOfWork =
                unitOfWork;

            _coverageEligibilityOptions =
                coverageEligibilityOptions.Value;
        }


        public async Task<ClaimDetailDto>
            CreateAsync(
                CreateClaimRequest request,
                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateCurrentUser();

            ValidateIncidentType(request.Type);

            ValidateCoverageRequests(
                request.Coverages);


            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            var policy =
                await _policyServiceClient
                    .GetPolicyAsync(
                        request.PolicyId,
                        cancellationToken);


            if (policy is null)
            {
                throw new NotFoundException(
                    "Policy was not found.");
            }


            ValidatePolicyForClaim(
                policy.CustomerId,
                policy.PropertyId,
                policy.IsActive,
                policy.EffectiveDateUtc,
                policy.ExpiryDateUtc,
                customerId,
                request.PropertyId,
                request.IncidentDate);


            var policyCoverages =
                policy.Coverages.ToDictionary(
                    coverage =>
                        coverage.CoverageId);


            ClaimCoverage[] claimCoverages =
                new ClaimCoverage[
                    request.Coverages.Count];


            int coverageIndex = 0;


            foreach (
                ClaimCoverageRequest
                    requestedCoverage
                in request.Coverages)
            {
                if (
                    !policyCoverages.TryGetValue(
                        requestedCoverage.CoverageId,
                        out var policyCoverage)
                )
                {
                    throw new BadRequestException(
                        "One or more selected Coverages do not belong to the selected Policy.");
                }

                EnsureCoverageEligible(
                    request.Type,
                    policy.ProductType,
                    policyCoverage.CoverageId,
                    policyCoverage.CoverageName,
                    policy.Coverages.Select(
                        coverage => coverage.CoverageId));

                decimal committedAmount =
                    await _claimRepository
                        .GetCommittedAmountAsync(
                            request.PolicyId,
                            requestedCoverage.CoverageId,
                            excludingClaimId: null,
                            cancellationToken);


                decimal remainingLimit =
                    policyCoverage
                        .CoverageLimitAmount -
                    committedAmount;


                if (remainingLimit < 0)
                {
                    remainingLimit = 0;
                }


                if (
                    requestedCoverage.RequestedAmount >
                    remainingLimit
                )
                {
                    throw new BadRequestException(
                        $"The requested amount for Coverage " +
                        $"'{policyCoverage.CoverageName}' exceeds " +
                        $"the remaining available limit of " +
                        $"{remainingLimit:0.00}.");
                }


                claimCoverages[
                    coverageIndex] =
                        new ClaimCoverage
                        {
                            Id =
                                Guid.NewGuid(),


                            CoverageId =
                                policyCoverage
                                    .CoverageId,


                            CoverageName =
                                policyCoverage
                                    .CoverageName,


                            IsMandatoryCoverage =
                                policyCoverage
                                    .IsMandatory,


                            CoverageCharge =
                                policyCoverage
                                    .CoverageCharge,


                            CoverageLimitPercentage =
                                policyCoverage
                                    .CoverageLimitPercentage,


                            CoverageLimitAmount =
                                policyCoverage
                                    .CoverageLimitAmount,


                            RequestedAmount =
                                requestedCoverage
                                    .RequestedAmount,


                            ApprovedAmount =
                                null
                        };


                coverageIndex++;
            }


            decimal claimedAmount =
                claimCoverages.Sum(
                    coverage =>
                        coverage.RequestedAmount);


            if (claimedAmount <= 0)
            {
                throw new BadRequestException(
                    "The total claimed amount must be greater than zero.");
            }


            long sequence =
                await _sequenceRepository
                    .GetNextClaimNumberAsync(
                        cancellationToken);


            string claimNumber =
                $"CLM-{DateTime.UtcNow:yyyy}-" +
                $"{sequence:D6}";


            DateTime utcNow =
                DateTime.UtcNow;


            var claim =
                new Claim
                {
                    Id =
                        Guid.NewGuid(),


                    ClaimNumber =
                        claimNumber,


                    CustomerId =
                       customerId,


                    PolicyId =
                        request.PolicyId,


                    PolicyNumber =
                        policy.PolicyNumber,


                    PropertyId =
                        request.PropertyId,


                    Type =
                        request.Type,


                    Status =
                        ClaimStatus.Submitted,


                    IncidentDate =
                        request.IncidentDate,


                    SubmittedAtUtc =
                        utcNow,


                    Title =
                        request.Title.Trim(),


                    Description =
                        request.Description.Trim(),


                    ClaimedAmount =
                        claimedAmount,


                    ApprovedAmount =
                        null,


                    CreatedBy =
                        _currentUserService.Email,


                    CreatedAtUtc =
                        utcNow
                };


            foreach (
                ClaimCoverage claimCoverage
                in claimCoverages)
            {
                claimCoverage.ClaimId =
                    claim.Id;


                claim.Coverages.Add(
                    claimCoverage);
            }


            claim.StatusHistory.Add(
                new ClaimStatusHistory
                {
                    Id = Guid.NewGuid(),
                    ClaimId = claim.Id,
                    PreviousStatus = ClaimStatus.Submitted,
                    NewStatus = ClaimStatus.Submitted,
                    Comments = "Claim submitted.",
                    ChangedBy = _currentUserService.Email,
                    ChangedAtUtc = utcNow
                });


            await _claimRepository.AddAsync(
                claim,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "CLAIM_CREATED",
                $"Claim {claim.ClaimNumber} created " +
                $"with {claim.Coverages.Count} Coverages " +
                $"and total requested amount " +
                $"{claim.ClaimedAmount:0.00}.",
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);


            return claim.ToDetailDto();
        }

        public async Task<ClaimCoverageEligibilityDto>
            GetEligibleCoveragesAsync(
                Guid policyId,
                ClaimType incidentType,
                CancellationToken cancellationToken = default)
        {
            ValidateCurrentUser();
            ValidateIncidentType(incidentType);

            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);

            var policy =
                await _policyServiceClient
                    .GetPolicyAsync(
                        policyId,
                        cancellationToken);

            if (policy is null)
            {
                throw new NotFoundException(
                    "Policy was not found.");
            }

            if (policy.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "The selected Policy does not belong to the current Customer.");
            }

            if (!policy.IsActive)
            {
                throw new BadRequestException(
                    "Only an active Policy can be used to submit a Claim.");
            }

            if (!ClaimCoverageEligibilityRules.IsConfigured(
                    _coverageEligibilityOptions,
                    policy.ProductType,
                    incidentType))
            {
                return new ClaimCoverageEligibilityDto(
                    false,
                    $"Coverage eligibility rules for {policy.ProductType} and {incidentType} have not been configured. No coverage is assumed.",
                    Array.Empty<EligibleClaimCoverageDto>());
            }

            var eligibleCoverages =
                new List<EligibleClaimCoverageDto>();
            IReadOnlyCollection<Guid> eligibleCoverageIds =
                ClaimCoverageEligibilityRules
                    .GetEligibleCoverageIds(
                        _coverageEligibilityOptions,
                        policy.ProductType,
                        incidentType,
                        policy.Coverages.Select(
                            coverage => coverage.CoverageId));

            foreach (var policyCoverage in policy.Coverages)
            {
                if (!eligibleCoverageIds.Contains(
                        policyCoverage.CoverageId))
                {
                    continue;
                }

                decimal committedAmount =
                    await _claimRepository
                        .GetCommittedAmountAsync(
                            policyId,
                            policyCoverage.CoverageId,
                            excludingClaimId: null,
                            cancellationToken);

                decimal remainingLimit =
                    Math.Max(
                        0,
                        policyCoverage.CoverageLimitAmount -
                        committedAmount);

                if (remainingLimit <= 0)
                {
                    continue;
                }

                eligibleCoverages.Add(
                    new EligibleClaimCoverageDto(
                        policyCoverage.CoverageId,
                        policyCoverage.CoverageName,
                        policyCoverage.CoverageLimitAmount,
                        remainingLimit));
            }

            string message =
                eligibleCoverages.Count == 0
                    ? "No coverages on this Policy are configured as eligible for the selected incident type, or no limit remains."
                    : string.Empty;

            return new ClaimCoverageEligibilityDto(
                true,
                message,
                eligibleCoverages);
        }


        public async Task<ClaimDetailDto>
            GetAsync(
                Guid claimId,
                CancellationToken cancellationToken = default)
        {
            ValidateCurrentUser();


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
                    "Claim was not found.");
            Guid customerId =
    await _customerIdentityService
        .GetCurrentCustomerIdAsync(
            cancellationToken);
            if (claim.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "Access denied.");
            }


            return claim.ToDetailDto();
        }


        public async Task<
            PagedResult<ClaimSummaryDto>>
            SearchAsync(
                ClaimSearchRequest request,
                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateCurrentUser();


            ValidatePagination(
                request.PageNumber,
                request.PageSize);


            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            PagedResult<Claim> result =
        await _claimRepository.SearchAsync(
            customerId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);


            IEnumerable<Claim> filteredItems =
                result.Items;


            if (
                !string.IsNullOrWhiteSpace(
                    request.ClaimNumber)
            )
            {
                string claimNumber =
                    request.ClaimNumber.Trim();


                filteredItems =
                    filteredItems.Where(
                        claim =>
                            claim.ClaimNumber.Contains(
                                claimNumber,
                                StringComparison
                                    .OrdinalIgnoreCase));
            }


            if (request.Status.HasValue)
            {
                filteredItems =
                    filteredItems.Where(
                        claim =>
                            claim.Status ==
                            request.Status.Value);
            }


            ClaimSummaryDto[] items =
                filteredItems
                    .Select(
                        ClaimMapper.ToSummaryDto)
                    .ToArray();


            return new PagedResult<
                ClaimSummaryDto>
            {
                PageNumber =
                    result.PageNumber,


                PageSize =
                    result.PageSize,


                TotalCount =
                    result.TotalCount,


                Items =
                    items
            };
        }


        public async Task UpdateAsync(
            Guid claimId,
            UpdateClaimRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateCurrentUser();

            ValidateCoverageRequests(
                request.Coverages);


            if (claimId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Claim identifier is required.");
            }


            Claim claim =
                await _claimRepository.GetByIdAsync(
                    claimId,
                    track: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim was not found.");

            if (
                !request.Type.IsSelectableIncidentType() &&
                request.Type != claim.Type
            )
            {
                throw new BadRequestException(
                    "A legacy Claim type cannot be selected for a new incident.");
            }


            Guid customerId =
                await _customerIdentityService
        .GetCurrentCustomerIdAsync(
            cancellationToken);
            if (claim.CustomerId != customerId
            )
            {
                throw new ForbiddenException(
                    "Access denied.");
            }


            if (
                claim.Status !=
                    ClaimStatus.Submitted &&
                claim.Status !=
                    ClaimStatus
                        .AdditionalInformationRequested
            )
            {
                throw new BadRequestException(
                    "Only submitted Claims or Claims awaiting additional information can be edited.");
            }


            _claimRepository
                .SetOriginalRowVersion(
                    claim,
                    request.RowVersion);


            var policy =
                await _policyServiceClient
                    .GetPolicyAsync(
                        claim.PolicyId,
                        cancellationToken);


            if (policy is null)
            {
                throw new NotFoundException(
                    "The Claim Policy was not found.");
            }


            ValidatePolicyForClaim(
                policy.CustomerId,
                policy.PropertyId,
                policy.IsActive,
                policy.EffectiveDateUtc,
                policy.ExpiryDateUtc,
                claim.CustomerId,
                claim.PropertyId,
                request.IncidentDate);


            var policyCoverages =
                policy.Coverages.ToDictionary(
                    coverage =>
                        coverage.CoverageId);


            List<ClaimCoverage>
                replacementCoverages =
                    new();


            foreach (
                ClaimCoverageRequest
                    requestedCoverage
                in request.Coverages)
            {
                if (
                    !policyCoverages.TryGetValue(
                        requestedCoverage.CoverageId,
                        out var policyCoverage)
                )
                {
                    throw new BadRequestException(
                        "One or more selected Coverages do not belong to the Claim Policy.");
                }

                if (request.Type.IsSelectableIncidentType())
                {
                    EnsureCoverageEligible(
                        request.Type,
                        policy.ProductType,
                        policyCoverage.CoverageId,
                        policyCoverage.CoverageName,
                        policy.Coverages.Select(
                            coverage => coverage.CoverageId));
                }

                decimal committedAmount =
                    await _claimRepository
                        .GetCommittedAmountAsync(
                            claim.PolicyId,
                            requestedCoverage.CoverageId,
                            excludingClaimId:
                                claim.Id,
                            cancellationToken);


                decimal remainingLimit =
                    policyCoverage
                        .CoverageLimitAmount -
                    committedAmount;


                if (remainingLimit < 0)
                {
                    remainingLimit = 0;
                }


                if (
                    requestedCoverage.RequestedAmount >
                    remainingLimit
                )
                {
                    throw new BadRequestException(
                        $"The requested amount for Coverage " +
                        $"'{policyCoverage.CoverageName}' exceeds " +
                        $"the remaining available limit of " +
                        $"{remainingLimit:0.00}.");
                }


                replacementCoverages.Add(
                    new ClaimCoverage
                    {
                        Id =
                            Guid.NewGuid(),


                        ClaimId =
                            claim.Id,


                        CoverageId =
                            policyCoverage.CoverageId,


                        CoverageName =
                            policyCoverage.CoverageName,


                        IsMandatoryCoverage =
                            policyCoverage.IsMandatory,


                        CoverageCharge =
                            policyCoverage.CoverageCharge,


                        CoverageLimitPercentage =
                            policyCoverage
                                .CoverageLimitPercentage,


                        CoverageLimitAmount =
                            policyCoverage
                                .CoverageLimitAmount,


                        RequestedAmount =
                            requestedCoverage
                                .RequestedAmount,


                        ApprovedAmount =
                            null
                    });
            }


            decimal claimedAmount =
                replacementCoverages.Sum(
                    coverage =>
                        coverage.RequestedAmount);


            if (claimedAmount <= 0)
            {
                throw new BadRequestException(
                    "The total claimed amount must be greater than zero.");
            }


            claim.Coverages.Clear();


            foreach (
                ClaimCoverage coverage
                in replacementCoverages)
            {
                claim.Coverages.Add(
                    coverage);
            }


            claim.Type =
                request.Type;


            claim.IncidentDate =
                request.IncidentDate;


            claim.Title =
                request.Title.Trim();


            claim.Description =
                request.Description.Trim();


            claim.ClaimedAmount =
                claimedAmount;


            claim.ApprovedAmount =
                null;


            claim.LastModifiedAtUtc =
                DateTime.UtcNow;


            claim.LastModifiedBy =
                _currentUserService.Email;


            await _claimRepository.UpdateAsync(
                claim,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "CLAIM_UPDATED",
                $"Claim updated with " +
                $"{claim.Coverages.Count} Coverages " +
                $"and total requested amount " +
                $"{claim.ClaimedAmount:0.00}.",
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        public async Task WithdrawAsync(
            Guid claimId,
            WithdrawClaimRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidateCurrentUser();


            if (claimId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Claim identifier is required.");
            }


            Claim claim =
                await _claimRepository.GetByIdAsync(
                    claimId,
                    track: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim was not found.");


            if (
                claim.CustomerId !=
                _currentUserService.UserId
            )
            {
                throw new ForbiddenException(
                    "Access denied.");
            }


            _claimRepository
                .SetOriginalRowVersion(
                    claim,
                    request.RowVersion);


            if (
                claim.Status ==
                    ClaimStatus.Approved ||
                claim.Status ==
                    ClaimStatus.Paid ||
                claim.Status ==
                    ClaimStatus.Closed ||
                claim.Status ==
                    ClaimStatus.Rejected ||
                claim.Status ==
                    ClaimStatus.Withdrawn
            )
            {
                throw new BadRequestException(
                    "The Claim cannot be withdrawn in its current status.");
            }


            ClaimStatus previousStatus =
                claim.Status;


            DateTime utcNow = DateTime.UtcNow;


            claim.Status =
                ClaimStatus.Withdrawn;


            claim.LastModifiedAtUtc =
                utcNow;


            claim.LastModifiedBy =
                _currentUserService.Email;


            _claimRepository.AddStatusHistory(
                new ClaimStatusHistory
                {
                    Id = Guid.NewGuid(),
                    ClaimId = claim.Id,
                    PreviousStatus = previousStatus,
                    NewStatus = ClaimStatus.Withdrawn,
                    Comments = string.IsNullOrWhiteSpace(request.Reason)
                        ? "Withdrawn by Customer."
                        : request.Reason.Trim(),
                    ChangedBy = _currentUserService.Email,
                    ChangedAtUtc = utcNow
                });


            await _claimRepository.UpdateAsync(
                claim,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "CLAIM_WITHDRAWN",
                request.Reason,
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        private void ValidateCurrentUser()
        {
            if (
                _currentUserService.UserId ==
                Guid.Empty
            )
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
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


        private void EnsureCoverageEligible(
            ClaimType incidentType,
            ProductType productType,
            Guid coverageId,
            string coverageName,
            IEnumerable<Guid> policyCoverageIds)
        {
            if (!ClaimCoverageEligibilityRules.IsConfigured(
                    _coverageEligibilityOptions,
                    productType,
                    incidentType))
            {
                throw new BadRequestException(
                    $"Coverage eligibility rules for {productType} and {incidentType} have not been configured. No coverage is assumed.");
            }

            if (!ClaimCoverageEligibilityRules.IsEligible(
                    _coverageEligibilityOptions,
                    productType,
                    incidentType,
                    coverageId,
                    policyCoverageIds))
            {
                throw new BadRequestException(
                    $"Coverage '{coverageName}' is not configured as eligible for {incidentType}.");
            }
        }

        private static void ValidateIncidentType(
            ClaimType incidentType)
        {
            if (!incidentType.IsSelectableIncidentType())
            {
                throw new BadRequestException(
                    "Select one of the supported incident types: Fire, Flood, Theft, Earthquake, Wildfire, Hurricane, or Other.");
            }
        }


        private static void
            ValidateCoverageRequests(
                IReadOnlyCollection<
                    ClaimCoverageRequest>
                    coverages)
        {
            if (
                coverages is null ||
                coverages.Count == 0
            )
            {
                throw new BadRequestException(
                    "At least one Policy Coverage must be selected.");
            }


            if (
                coverages.Any(
                    coverage =>
                        coverage.CoverageId ==
                            Guid.Empty)
            )
            {
                throw new BadRequestException(
                    "Every selected Coverage must have a valid identifier.");
            }


            if (
                coverages.Any(
                    coverage =>
                        coverage.RequestedAmount <=
                            0)
            )
            {
                throw new BadRequestException(
                    "Every selected Coverage amount must be greater than zero.");
            }


            bool hasDuplicateCoverage =
                coverages
                    .GroupBy(
                        coverage =>
                            coverage.CoverageId)
                    .Any(
                        group =>
                            group.Count() > 1);


            if (hasDuplicateCoverage)
            {
                throw new BadRequestException(
                    "A Policy Coverage cannot be selected more than once.");
            }
        }


        private static void
            ValidatePolicyForClaim(
                Guid policyCustomerId,
                Guid policyPropertyId,
                bool policyIsActive,
                DateTime policyEffectiveDate,
                DateTime policyExpirationDate,
                Guid customerId,
                Guid propertyId,
                DateTime incidentDate)
        {
            if (!policyIsActive)
            {
                throw new BadRequestException(
                    "Only an active Policy can be used to submit a Claim.");
            }


            if (
                policyCustomerId !=
                customerId
            )
            {
                throw new ForbiddenException(
                    "The selected Policy does not belong to the current Customer.");
            }


            if (
                policyPropertyId !=
                propertyId
            )
            {
                throw new BadRequestException(
                    "The selected Property does not match the Policy Property.");
            }


            if (
                incidentDate.Date >
                DateTime.UtcNow.Date
            )
            {
                throw new BadRequestException(
                    "Incident date cannot be in the future.");
            }


            if (
                incidentDate.Date <
                    policyEffectiveDate.Date ||
                incidentDate.Date >
                    policyExpirationDate.Date
            )
            {
                throw new BadRequestException(
                    "Incident date is outside the Policy period.");
            }
        }


        private static void ValidatePagination(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new BadRequestException(
                    "Page number must be greater than zero.");
            }


            if (
                pageSize < 1 ||
                pageSize > 100
            )
            {
                throw new BadRequestException(
                    "Page size must be between 1 and 100.");
            }
        }
    }
}
