using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.DTOs.Internal;
using VacationHome.PolicyService.Application.DTOs.Policies;
using VacationHome.PolicyService.Application.Exceptions;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Application.Interfaces.Services;
using VacationHome.PolicyService.Application.Mappers;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.Services
{
    public sealed class PoliciesService
        : IPolicyService
    {
        private readonly IPolicyRepository
            _policyRepository;


        private readonly IAuditService
            _auditService;


        private readonly INotificationService
            _notificationService;


        public PoliciesService(
            IPolicyRepository policyRepository,
            IAuditService auditService,
            INotificationService notificationService)
        {
            _policyRepository =
                policyRepository;


            _auditService =
                auditService;


            _notificationService =
                notificationService;
        }


        public async Task<PolicyDetailsDto>
            GetAsync(
                Guid policyId,
                Guid customerId,
                bool isAdmin,
                CancellationToken cancellationToken)
        {
            if (policyId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Policy identifier is required.");
            }


            Policy? policy =
                await _policyRepository.GetByIdAsync(
                    policyId,
                    track: false,
                    cancellationToken);


            if (policy is null)
            {
                throw new NotFoundException(
                    "Policy not found.");
            }


            if (
                !isAdmin &&
                policy.CustomerId != customerId
            )
            {
                throw new ForbiddenException(
                    "Access denied.");
            }


            return PolicyMapper.ToDetailsDto(
                policy);
        }


        public async Task<
            IReadOnlyCollection<PolicySummaryDto>>
            GetCustomerPoliciesAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            if (customerId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Customer identifier is required.");
            }


            IReadOnlyList<Policy> policies =
                await _policyRepository
                    .GetCustomerPoliciesAsync(
                        customerId,
                        cancellationToken);


            return policies
                .Select(
                    PolicyMapper.ToSummaryDto)
                .ToArray();
        }


        public async Task<
            PagedResult<PolicySummaryDto>>
            GetAdminPoliciesAsync(
                PolicyFilterRequest request,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidatePolicyFilter(
                request);


            PagedResult<Policy> result =
                await _policyRepository
                    .GetAdminPoliciesAsync(
                        request,
                        cancellationToken);


            return new PagedResult<
                PolicySummaryDto>(
                    result.Items
                        .Select(
                            PolicyMapper
                                .ToSummaryDto)
                        .ToArray(),
                    result.PageNumber,
                    result.PageSize,
                    result.TotalCount);
        }


        public async Task CancelAsync(
            Guid policyId,
            Guid customerId,
            CancelPolicyRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            if (policyId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Policy identifier is required.");
            }


            if (customerId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Customer identifier is required.");
            }


            if (
                string.IsNullOrWhiteSpace(
                    request.Reason)
            )
            {
                throw new BadRequestException(
                    "Cancellation reason is required.");
            }


            Policy? policy =
                await _policyRepository.GetByIdAsync(
                    policyId,
                    track: true,
                    cancellationToken);


            if (policy is null)
            {
                throw new NotFoundException(
                    "Policy not found.");
            }


            if (
                policy.CustomerId !=
                customerId
            )
            {
                throw new ForbiddenException(
                    "Policy ownership mismatch.");
            }


            if (
                policy.Status !=
                PolicyStatus.Active
            )
            {
                throw new BadRequestException(
                    "Only active policies can be cancelled.");
            }


            DateTime utcNow =
                DateTime.UtcNow;


            policy.Status =
                PolicyStatus.Cancelled;


            policy.CancellationReason =
                request.Reason.Trim();


            policy.CancelledAtUtc =
                utcNow;


            policy.UpdatedAtUtc =
                utcNow;


            await _policyRepository.UpdateAsync(
                policy,
                cancellationToken);


            await _policyRepository.SaveChangesAsync(
                cancellationToken);


            await _auditService.LogAsync(
                "POLICY_CANCELLED",
                "Policy",
                policy.Id.ToString(),
                policy.CancellationReason,
                cancellationToken);


            try
            {
                await _notificationService.SendAsync(
                    policy.CustomerId,
                    "Policy Cancelled",
                    $"Policy {policy.PolicyNumber} has been cancelled.",
                    cancellationToken);
            }
            catch
            {
                /*
                 * Notification failure must not reverse
                 * the committed Policy cancellation.
                 */
            }
        }


        public async Task ExpirePoliciesAsync(
            CancellationToken cancellationToken)
        {
            DateTime utcNow =
                DateTime.UtcNow;


            IReadOnlyList<Policy> policies =
                await _policyRepository
                    .GetPoliciesForExpiryAsync(
                        utcNow,
                        cancellationToken);


            if (policies.Count == 0)
            {
                return;
            }


            foreach (Policy policy in policies)
            {
                policy.Status =
                    PolicyStatus.Expired;


                policy.UpdatedAtUtc =
                    utcNow;


                await _auditService.LogAsync(
                    "POLICY_EXPIRED",
                    "Policy",
                    policy.Id.ToString(),
                    policy.PolicyNumber,
                    cancellationToken);


                try
                {
                    await _notificationService.SendAsync(
                        policy.CustomerId,
                        "Policy Expired",
                        $"Policy {policy.PolicyNumber} has expired.",
                        cancellationToken);
                }
                catch
                {
                    /*
                     * Notification failure must not stop
                     * the Policy expiry process.
                     */
                }
            }


            await _policyRepository.SaveChangesAsync(
                cancellationToken);
        }


        public async Task<PolicyOverlapResultDto>
            CheckOverlapAsync(
                PolicyOverlapRequestDto request,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            if (request.CustomerId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Customer identifier is required.");
            }


            if (request.PropertyId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Property identifier is required.");
            }


            IReadOnlyList<Policy> policies =
                await _policyRepository
                    .GetActivePoliciesAsync(
                        request.CustomerId,
                        request.PropertyId,
                        request.ExcludedQuoteId,
                        cancellationToken);
            Policy[] activePolicies =
                policies
                    .Where(
                        policy =>
                            policy.CustomerId ==
                                request.CustomerId &&
                            policy.PropertyId ==
                                request.PropertyId &&
                            policy.Status ==
                                PolicyStatus.Active)
                    .ToArray();

            bool duplicateProductExists =
                activePolicies.Any(
                    policy =>
                        policy.ProductType ==
                        request.ProductType);


            if (duplicateProductExists)
            {
                return new PolicyOverlapResultDto(
                    true,
                    "DUPLICATE_PRODUCT_POLICY",
                    "This property already has an active policy for the selected product.");
            }
            if (
                request.ProductType ==
                ProductType.ComprehensiveProtection
            )
            {
                if (activePolicies.Length > 0)
                {
                    return new PolicyOverlapResultDto(
                        true,
                        "COMPREHENSIVE_POLICY_CONFLICT",
                        "Property Protection and Natural Disaster Protection may coexist, but neither may coexist with Comprehensive Protection on the same property.");
                }
            }
            else
            {
                bool hasComprehensivePolicy =
                    activePolicies.Any(
                        policy =>
                            policy.ProductType ==
                            ProductType
                                .ComprehensiveProtection);


                if (hasComprehensivePolicy)
                {
                    return new PolicyOverlapResultDto(
                        true,
                        "COMPREHENSIVE_POLICY_EXISTS",
                        "Comprehensive Protection cannot coexist with Property Protection or Natural Disaster Protection on the same property.");
                }
            }


            return new PolicyOverlapResultDto(
                false,
                null,
                null);
        }


        public async Task<
            CoverageValidationResultDto>
            ValidateClaimCoverageAsync(
                Guid policyId,
                DateTime incidentDate,
                CancellationToken cancellationToken)
        {
            if (policyId == Guid.Empty)
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Policy identifier is required."
                };
            }


            Policy? policy =
                await _policyRepository.GetByIdAsync(
                    policyId,
                    track: false,
                    cancellationToken);


            if (policy is null)
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Policy not found."
                };
            }


            if (
                policy.Status !=
                PolicyStatus.Active
            )
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Policy is not active."
                };
            }


            if (
                incidentDate >
                DateTime.UtcNow
            )
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Incident date cannot be in the future."
                };
            }


            if (
                incidentDate <
                    policy.EffectiveDateUtc ||
                incidentDate >
                    policy.ExpiryDateUtc
            )
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Incident date is outside the Policy period."
                };
            }


            if (policy.Coverages.Count == 0)
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Policy does not contain any Coverages."
                };
            }


            return new CoverageValidationResultDto
            {
                IsCovered =
                    true
            };
        }


        public async Task<
            CoverageValidationResultDto>
            ValidateClaimAmountAsync(
                Guid policyId,
                decimal claimAmount,
                CancellationToken cancellationToken)
        {
            if (policyId == Guid.Empty)
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Policy identifier is required."
                };
            }


            if (claimAmount <= 0)
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Claim amount must be greater than zero."
                };
            }


            Policy? policy =
                await _policyRepository.GetByIdAsync(
                    policyId,
                    track: false,
                    cancellationToken);


            if (policy is null)
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Policy not found."
                };
            }


            if (
                policy.Status !=
                PolicyStatus.Active
            )
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Policy is not active."
                };
            }


            decimal totalCoverageLimit =
                policy.Coverages.Sum(
                    coverage =>
                        coverage.CoverageLimitAmount);


            if (
                claimAmount >
                totalCoverageLimit
            )
            {
                return new CoverageValidationResultDto
                {
                    IsCovered =
                        false,


                    Reason =
                        "Claim amount exceeds the combined Policy Coverage limit.",


                    MaximumClaimAmount =
                        totalCoverageLimit
                };
            }


            return new CoverageValidationResultDto
            {
                IsCovered =
                    true,


                MaximumClaimAmount =
                    totalCoverageLimit
            };
        }


        private static void ValidatePolicyFilter(
            PolicyFilterRequest request)
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
                !string.IsNullOrWhiteSpace(
                    request.ProductName) &&
                request.ProductName.Trim().Length >
                    200
            )
            {
                throw new BadRequestException(
                    "Product name filter cannot exceed 200 characters.");
            }


            if (
                request.Status.HasValue &&
                !Enum.IsDefined(
                    request.Status.Value)
            )
            {
                throw new BadRequestException(
                    "Policy status filter is invalid.");
            }


            if (
                request.ProductType.HasValue &&
                !Enum.IsDefined(
                    request.ProductType.Value)
            )
            {
                throw new BadRequestException(
                    "Product type filter is invalid.");
            }
        }
    }
}
