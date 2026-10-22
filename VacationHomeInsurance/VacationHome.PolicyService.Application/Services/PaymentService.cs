using System;
using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.DTOs.External;
using VacationHome.PolicyService.Application.DTOs.Documents;
using VacationHome.PolicyService.Application.DTOs.Payments;
using VacationHome.PolicyService.Application.Exceptions;
using VacationHome.PolicyService.Application.Interfaces.External;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Application.Interfaces.Services;
using VacationHome.PolicyService.Application.Mappers;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.Services
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IQuoteServiceClient _quoteClient;
        private readonly INumberGenerator _numberGenerator;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly ICustomerServiceClient _customerServiceClient;
        private readonly IPolicyDocumentService _policyDocumentService;


        public PaymentService(
            IPaymentRepository paymentRepository,
            IPolicyRepository policyRepository,
            IQuoteServiceClient quoteClient,
            INumberGenerator numberGenerator,
            IAuditService auditService,
            INotificationService notificationService,
            ICustomerServiceClient customerServiceClient,
            IPolicyDocumentService policyDocumentService)
        {
            _paymentRepository = paymentRepository;
            _policyRepository = policyRepository;
            _quoteClient = quoteClient;
            _numberGenerator = numberGenerator;
            _auditService = auditService;
            _notificationService = notificationService;
            _customerServiceClient = customerServiceClient;
            _policyDocumentService = policyDocumentService;
        }


        public async Task<PaymentResultDto> ProcessPaymentAsync(
          Guid customerId,
          PaymentRequest request,
          CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            if (customerId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Customer identifier is required.");
            }


            if (request.QuoteId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Quote identifier is required.");
            }


            /*
             * Payment success is idempotent.
             *
             * If a successful Payment already exists for the Quote,
             * return the existing result instead of creating another
             * Payment or Policy.
             */
            var existingSuccess =
              await _paymentRepository
                 .GetSuccessfulPaymentAsync(
           request.QuoteId,
             cancellationToken);


            if (existingSuccess is not null)
            {
                if (existingSuccess.CustomerId != customerId)
                {
                    throw new ForbiddenException(
                        "The payment does not belong to the authenticated customer.");
                }

                if (!existingSuccess.PolicyId.HasValue)
                {
                    throw new ConflictException(
                        "The successful payment does not have a linked Policy.");
                }

                await _quoteClient.MarkQuoteConvertedAsync(
                    request.QuoteId,
                    customerId,
                    existingSuccess.PolicyId.Value,
                    existingSuccess.Id,
                    existingSuccess.Amount,
                    existingSuccess.ProcessedAtUtc,
                    rowVersion: string.Empty,
                    cancellationToken);


                var existingDocument =
                    await _policyDocumentService
                        .EnsureLatestGeneratedAsync(
                            existingSuccess.PolicyId.Value,
                            cancellationToken);

                return new PaymentResultDto(
                    existingSuccess.Id,
                    existingSuccess.PaymentNumber,
                    existingSuccess.Status,
                    existingSuccess.PolicyId,
                    "Payment already completed.",
                    existingDocument);
            }


            var attempts =
                await _paymentRepository
                    .GetAttemptCountAsync(
                        request.QuoteId,
                        cancellationToken);


            if (attempts >= 5)
            {
                throw new PaymentAttemptsExceededException();
            }


            /*
             * ProductQuote Service verifies:
             *
             * - Quote ownership
             * - Customer acceptance
             * - Admin approval
             * - Quote status
             * - Quote expiry
             * - Payment deadline
             * - Property snapshot
             * - Product overlap
             * - Conversion eligibility
             */
            var quote =
                await _quoteClient.GetApprovedQuoteAsync(
        request.QuoteId,
              customerId,
                    cancellationToken);


            if (quote.QuoteId == Guid.Empty)
            {
                throw new BadRequestException(
               "Quote Service returned an inv*lid quote.");
            }


            if (quote.CustomerId != customerId)
            {
                throw new ForbiddenException("The quote does not be*ong to the authenticated customer.");
            }


            if (!quote.IsEligibleForPayment)
            {
                throw new BadRequestException(
                    quote.IneligibleReason ??
                 "Quote is not eligible for payme*t.");
            }


            if (!quote.PaymentDeadlineUtc.HasValue)
            {
                throw new BadRequestException(
                    "The approved quote does not have a payment deadline.");
            }


            var currentTimeUtc =
                DateTime.UtcNow;


            if (currentTimeUtc >
                quote.PaymentDeadlineUtc.Value)
            {
                throw new BadRequestException(
                    "Payment deadline has expired.");
            }


            if (currentTimeUtc >
                quote.QuoteExpiryDateUtc)
            {
                throw new BadRequestException(
                    "Quote has expired.");
            }


            /*
             * The Customer cannot alter the payable amount.
             * It must exactly equal the annual premium stored in
             * the approved Quote snapshot.
             */
            if (quote.FinalAnnualPremium !=
                request.Amount)
            {
                await RecordFailedPaymentAsync(
                           customerId,
                      request,
                           attempts + 1,
                           "Payment amount mismatch.",
                           cancellationToken);


                throw new BadRequestException(
                    "Payment amount mismatch.");
            }


            var customerSnapshot =
                await _customerServiceClient.GetCustomerAsync(
                    customerId,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Customer details were not available for policy issuance.");


            if (!customerSnapshot.IsActive)
            {
                throw new ForbiddenException(
                    "An inactive Customer cannot issue a new Policy.");
            }


            var paymentNumber =
                await _numberGenerator
                    .GeneratePaymentNumberAsync(
                        cancellationToken);


            var paymentProcessedAtUtc =
                DateTime.UtcNow;


            var last4Digits =
                request.CardNumber.Length >= 4
                    ? request.CardNumber[^4..]
                    : string.Empty;


            /*
             * Store only safe mock-card metadata.
             *
             * The complete card number and CVV must never be
             * persisted or written to logs.
             */
            var payment = new Payment
          {
                Id = Guid.NewGuid(),


        PaymentNumber =paymentNumber,


        QuoteId =
            quote.QuoteId,


        CustomerId =
            customerId,


        Amount =
            request.Amount,


        AttemptNumber =
            attempts + 1,


        Status =
            PaymentStatus.Success,


        CardholderName =
            request.CardholderName,


        Last4Digits =
            last4Digits,


        ProcessedAtUtc =
            paymentProcessedAtUtc
    }
            ;


            await _paymentRepository.AddAsync(
                payment,
                cancellationToken);


            /*
             * CreatePolicyAsync is idempotent for the Quote.
             * If a Policy already exists for this Quote, the
             * existing Policy is returned.
             */
            var policy =
                await CreatePolicyAsync(
                    payment,
                    quote,
                    customerSnapshot,
                    cancellationToken);


            payment.PolicyId =
                policy.Id;


            /*
             * Persist the successful Payment and generated Policy
             * before notifying ProductQuote Service.
             *
             * This ensures that ProductQuote Service is never given
             * identifiers for records that have not been persisted.
             */
            await _policyRepository.SaveChangesAsync(
                cancellationToken);


            await _paymentRepository.SaveChangesAsync(
                cancellationToken);


            /*
             * ProductQuote Service expects:
             *
             * CustomerId
             * PolicyId
             * PaymentId
             * PaidAmount
             * PaymentSucceededAtUtc
             * CorrelationId
             * RowVersion
             *
             * CorrelationId is added by QuoteServiceClient from
             * HttpContext.TraceIdentifier.
             */
            await _quoteClient.MarkQuoteConvertedAsync(
                quote.QuoteId,
                customerId,
                policy.Id,
                payment.Id,
                payment.Amount,
                payment.ProcessedAtUtc,
                quote.RowVersion,
                cancellationToken);


            await _auditService.LogAsync(
                "PAYMENT_SUCCESS",
                "Payment",
                payment.Id.ToString(),
                $"Payment {payment.PaymentNumber} succeeded " +
                $"and Policy {policy.PolicyNumber} was activated " +
                $"for Quote {quote.QuoteNumber}.",
                cancellationToken);


   
            try
            {
                await _notificationService.SendAsync(
                    customerId,
                    "Policy Activated",
                    $"Policy {policy.PolicyNumber} has been generated.",
                    cancellationToken);
            }
            catch
            {
                /*
                 * Notification failure is intentionally non-blocking.
                 *
                 * Preferably log this failure inside
                 * NotificationService or add ILogger<PaymentService>.
                 */
            }


            PolicyDocumentDto policyDocument =
                await _policyDocumentService
                    .EnsureLatestGeneratedAsync(
                        policy.Id,
                        cancellationToken);


            return new PaymentResultDto(
                payment.Id,
                payment.PaymentNumber,
                PaymentStatus.Success,
                policy.Id,
                "Payment successful. Your policy document is ready to download.",
                policyDocument);
        }


        public async Task<PaymentDto> GetPaymentAsync(
            Guid paymentId,
            Guid customerId,
            bool isAdmin,
            CancellationToken cancellationToken)
        {
            var payment =
                await _paymentRepository.GetByIdAsync(
                    paymentId,
                    false,
                    cancellationToken);


            if (payment is null)
            {
                throw new NotFoundException(
                    "Payment not found.");
            }


            if (!isAdmin &&
                payment.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "Access denied.");
            }


            return PolicyMapper.ToDto(payment);
        }


        public async Task<IReadOnlyCollection<PaymentDto>>
            GetCustomerPaymentsAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            var payments =
                await _paymentRepository.GetCustomerPaymentsAsync(
                    customerId,
                    cancellationToken);


            return payments
                .Select(PolicyMapper.ToDto)
                .ToList();
        }


        public async Task<PagedResult<PaymentDto>>
            GetAdminPaymentsAsync(
                PaymentFilterRequest request,
                CancellationToken cancellationToken)
        {
            var result =
                await _paymentRepository.GetAdminPaymentsAsync(
                    request,
                    cancellationToken);


            return new PagedResult<PaymentDto>(
                result.Items
                    .Select(PolicyMapper.ToDto)
                    .ToList(),
                result.PageNumber,
                result.PageSize,
                result.TotalCount);
        }


        private async Task<Policy> CreatePolicyAsync(
            Payment payment,
            ApprovedQuoteDto quote,
            CustomerSnapshotDto customer,
            CancellationToken cancellationToken)
        {
            var existingPolicy =
                await _policyRepository.GetByQuoteIdAsync(
                    quote.QuoteId,
                    false,
                    cancellationToken);


            if (existingPolicy is not null)
            {
                return existingPolicy;
            }


            var policyNumber =
                await _numberGenerator.GeneratePolicyNumberAsync(
                    cancellationToken);


            var effectiveDate = DateTime.UtcNow;


            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = policyNumber,


                QuoteId = quote.QuoteId,


                CustomerId = quote.CustomerId,


                PropertyId = quote.PropertyId,
                PropertyName = quote.PropertyName,
                CustomerFirstName = customer.FirstName,
                CustomerLastName = customer.LastName,
                CustomerPhoneNumber = customer.PhoneNumber,
                PropertyAddressLine1 = quote.AddressLine1,
                PropertyAddressLine2 = quote.AddressLine2,
                PropertyCity = quote.City,
                PropertyState = quote.State,
                PropertyPostalCode = quote.PostalCode,
                PropertyCountry = quote.Country,
                PropertyType = quote.PropertyType,
                PropertyValue = quote.PropertyValue,
                ConstructionYear = quote.ConstructionYear,
                PropertyAge = quote.PropertyAge,
                OccupancyFrequency = quote.OccupancyFrequency,
                HasSecuritySystem = quote.HasSecuritySystem,
                PreviousClaimsCount = quote.PreviousClaimsCount,
                DisasterExposure = quote.HasDisasterExposure,
                RiskScore = quote.RiskScore,
                RiskLevel = quote.RiskLevel.ToString(),
                RiskExplanation = quote.RiskExplanation,


                PaymentId = payment.Id,


                ProductType = quote.ProductType,
                ProductName = quote.ProductName,


                AnnualPremium = quote.FinalAnnualPremium,
                BaseRate = quote.BaseRate,
                RiskMultiplier = quote.RiskMultiplier,
                BasePremium = quote.BasePremium,
                RiskAdjustedPremium = quote.RiskAdjustedPremium,
                RiskAdjustmentAmount = quote.RiskAdjustmentAmount,
                MandatoryCoverageCharges =
                    quote.MandatoryCoverageCharges,
                OptionalCoverageCharges =
                    quote.OptionalCoverageCharges,
                PaymentFrequency = quote.PaymentFrequency,
                EffectiveDateUtc = effectiveDate,
                ExpiryDateUtc = effectiveDate.AddYears(1),
                Status = PolicyStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };


            foreach (var coverage in quote.Coverages)
            {
                policy.Coverages.Add(
                    new PolicyCoverage
                    {
                        Id = Guid.NewGuid(),
                        PolicyId = policy.Id,
                        CoverageId = coverage.CoverageId,
                        CoverageName = coverage.CoverageName,
                        CoverageDescription =
                            coverage.CoverageDescription,
                        IsMandatory = coverage.IsMandatory,
                        CoverageCharge = coverage.CoverageCharge,
                        CoverageLimitPercentage =
                            coverage.CoverageLimitPercentage,
                        CoverageLimitAmount =
                            coverage.CoverageLimitAmount,
                        CreatedAtUtc = DateTime.UtcNow
                    });
            }


            await _policyRepository.AddAsync(
                policy,
                cancellationToken);


            return policy;
        }


        private async Task RecordFailedPaymentAsync(
            Guid customerId,
            PaymentRequest request,
            int attempt,
            string reason,
            CancellationToken cancellationToken)
        {
            var paymentNumber =
                await _numberGenerator.GeneratePaymentNumberAsync(
                    cancellationToken);


            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentNumber = paymentNumber,
                QuoteId = request.QuoteId,
                CustomerId = customerId,
                Amount = request.Amount,
                AttemptNumber = attempt,
                Status = PaymentStatus.Failed,
                CardholderName = request.CardholderName,
                Last4Digits = request.CardNumber.Length >= 4
                    ? request.CardNumber[^4..]
                    : string.Empty,
                FailureReason = reason,
                ProcessedAtUtc = DateTime.UtcNow
            };


            await _paymentRepository.AddAsync(
                payment,
                cancellationToken);


            await _paymentRepository.SaveChangesAsync(
                cancellationToken);


            await _auditService.LogAsync(
                "PAYMENT_FAILED",
                "Payment",
                payment.Id.ToString(),
                reason,
                cancellationToken);
        }
    }
}
