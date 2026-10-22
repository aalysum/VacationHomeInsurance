using VacationHome.PolicyService.Application.DTOs.Renewals;
using VacationHome.PolicyService.Application.Exceptions;
using VacationHome.PolicyService.Application.Interfaces.External;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Application.Interfaces.Services;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.Services
{
    public sealed class RenewalService : IRenewalService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IPolicyRenewalRepository _renewalRepository;
        private readonly IQuoteServiceClient _quoteClient;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;


        public RenewalService(
            IPolicyRepository policyRepository,
            IPolicyRenewalRepository renewalRepository,
            IQuoteServiceClient quoteClient,
            IAuditService auditService,
            INotificationService notificationService)
        {
            _policyRepository = policyRepository;
            _renewalRepository = renewalRepository;
            _quoteClient = quoteClient;
            _auditService = auditService;
            _notificationService = notificationService;
        }


        public async Task<RenewalEligibilityDto> CheckEligibilityAsync(
            Guid policyId,
            Guid customerId,
            CancellationToken cancellationToken)
        {
            var policy =
                await _policyRepository.GetByIdAsync(
                    policyId,
                    false,
                    cancellationToken);


            if (policy is null)
            {
                throw new NotFoundException(
                    "Policy not found.");
            }


            if (policy.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "Access denied.");
            }


            if (policy.Status == PolicyStatus.Cancelled)
            {
                return new RenewalEligibilityDto(
                    false,
                    "Cancelled policies cannot be renewed.");
            }


            var today = DateTime.UtcNow;


            var renewalOpenDate =
                policy.ExpiryDateUtc.AddDays(-30);


            var renewalCloseDate =
                policy.ExpiryDateUtc.AddDays(30);


            if (today < renewalOpenDate)
            {
                return new RenewalEligibilityDto(
                    false,
                    "Renewal window has not opened.");
            }


            if (today > renewalCloseDate)
            {
                return new RenewalEligibilityDto(
                    false,
                    "Renewal window has closed.");
            }


            return new RenewalEligibilityDto(
                true,
                "Policy is eligible for renewal.");
        }


        public async Task<RenewalResultDto> CreateRenewalAsync(
            Guid policyId,
            Guid customerId,
            CancellationToken cancellationToken)
        {
            var eligibility =
                await CheckEligibilityAsync(
                    policyId,
                    customerId,
                    cancellationToken);


            if (!eligibility.Eligible)
            {
                throw new BadRequestException(
                    eligibility.Reason);
            }


            var existingRenewal =
                await _renewalRepository.GetActiveAsync(
                    policyId,
                    cancellationToken);


            if (existingRenewal is not null)
            {
                throw new ConflictException(
                    "A renewal process already exists for this policy.");
            }


            var renewalQuote =
                await _quoteClient.CreateRenewalQuoteAsync(
                    policyId,
                    customerId,
                    cancellationToken);


            var renewal = new PolicyRenewal
            {
                Id = Guid.NewGuid(),
                ExistingPolicyId = policyId,
                CustomerId = customerId,
                RenewalQuoteId = renewalQuote.QuoteId,
                Status = RenewalStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            };


            await _renewalRepository.AddAsync(
                renewal,
                cancellationToken);


            await _renewalRepository.SaveChangesAsync(
                cancellationToken);


            await _auditService.LogAsync(
                "POLICY_RENEWAL_STARTED",
                "Policy",
                policyId.ToString(),
                $"Renewal quote created: {renewalQuote.QuoteId}",
                cancellationToken);


            try
            {
                await _notificationService.SendAsync(
                    customerId,
                    "Policy Renewal Started",
                    $"Renewal quote {renewalQuote.QuoteId} has been created.",
                    cancellationToken);
            }
            catch
            {
            }


            return new RenewalResultDto(
                renewal.Id,
                renewalQuote.QuoteId,
                "Renewal quote created successfully.");
        }


        public async Task CompleteRenewalAsync(
            Guid renewalQuoteId,
            Guid renewedPolicyId,
            CancellationToken cancellationToken)
        {
            var renewal =
                await _renewalRepository.GetByRenewalQuoteIdAsync(
                    renewalQuoteId,
                    true,
                    cancellationToken);


            if (renewal is null)
            {
                return;
            }


            if (renewal.Status == RenewalStatus.Completed)
            {
                return;
            }


            renewal.RenewedPolicyId = renewedPolicyId;
            renewal.Status = RenewalStatus.Completed;
            renewal.CompletedAtUtc = DateTime.UtcNow;


            await _renewalRepository.SaveChangesAsync(
                cancellationToken);


            await _auditService.LogAsync(
                "POLICY_RENEWAL_COMPLETED",
                "PolicyRenewal",
                renewal.Id.ToString(),
                renewedPolicyId.ToString(),
                cancellationToken);


            try
            {
                await _notificationService.SendAsync(
                    renewal.CustomerId,
                    "Policy Renewal Completed",
                    $"Renewal completed successfully.",
                    cancellationToken);
            }
            catch
            {
            }
        }
    }
}
