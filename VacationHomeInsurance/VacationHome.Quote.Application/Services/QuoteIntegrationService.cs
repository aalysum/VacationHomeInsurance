using System.Text.Json;
using VacationHome.ProductQuote.Application.DTOs.External;
using VacationHome.ProductQuote.Application.DTOs.Internal;
using VacationHome.ProductQuote.Application.DTOs.Quotes;
using VacationHome.ProductQuote.Application.Exceptions;
using VacationHome.ProductQuote.Application.Interfaces.External;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Application.Interfaces.Services;
using VacationHome.ProductQuote.Application.Mappers;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.Services
{
    public sealed class QuoteIntegrationService
        : IQuoteIntegrationService
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly ICustomerServiceClient _customerClient;
        private readonly IPolicyServiceClient _policyClient;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _productRepository;
        private readonly IRiskConfigurationRepository _riskConfigurationRepository;
        private readonly IPricingConfigurationRepository _pricingConfigurationRepository;


        private readonly IQuoteGenerationService
    _quoteGenerationService;
        public QuoteIntegrationService(
            IQuoteRepository quoteRepository,
            IProductRepository productRepository,
            IPricingConfigurationRepository pricingConfigurationRepository,
            IRiskConfigurationRepository riskConfigurationRepository,
            IQuoteGenerationService quoteGenerationService,
            ICustomerServiceClient customerClient,
            IPolicyServiceClient policyClient,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _quoteRepository = quoteRepository;
            _productRepository = productRepository;
            _pricingConfigurationRepository =
                pricingConfigurationRepository;


            _riskConfigurationRepository =
                riskConfigurationRepository;
            _quoteGenerationService =
              quoteGenerationService;
            _policyClient = policyClient;
            _customerClient = customerClient;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }


        public async Task<ValidatedQuoteDto>
            ValidateQuoteForPaymentAsync(
                ValidateQuoteForPaymentRequest request,
                CancellationToken cancellationToken)
        {
            DateTime utcNow = DateTime.UtcNow;


            Quote quote = await GetEligibleQuoteAsync(
                request.QuoteId,
                request.CustomerId,
                request.PaymentAmount,
                utcNow,
                cancellationToken);


            return quote.ToValidatedQuoteDto(utcNow);
        }


        public async Task<ValidatedQuoteDto>
            GetValidatedQuoteAsync(
                Guid quoteId,
                Guid customerId,
                string correlationId,
                CancellationToken cancellationToken)
        {
            Quote quote =
                await GetEligibleQuoteAsync(
                    quoteId,
                    customerId,
                    paymentAmount: null,
                    DateTime.UtcNow,
                    cancellationToken);


            return quote.ToValidatedQuoteDto(
                DateTime.UtcNow);
        }


        public async Task<QuoteConversionResultDto>
            MarkQuoteConvertedAsync(
                Guid quoteId,
                MarkQuoteConvertedRequest request,
                CancellationToken cancellationToken)
        {
            DateTime utcNow = DateTime.UtcNow;


            Quote quote =
                await _quoteRepository.GetDetailsAsync(
                    quoteId,
                    tracking: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");


            if (quote.CustomerId != request.CustomerId)
            {
                throw new ForbiddenException(
                    "The Customer does not own the Quote.");
            }


            if (quote.Status ==
                    QuoteStatus.ConvertedToPolicy &&
                quote.ConvertedPolicyId ==
                    request.PolicyId)
            {
                return quote.ToConversionResultDto(
                    wasAlreadyConverted: true);
            }


            if (quote.Status ==
                    QuoteStatus.ConvertedToPolicy ||
                quote.ConvertedPolicyId.HasValue)
            {
                throw new ConflictException(
                    "QUOTE_ALREADY_CONVERTED",
                    "The Quote was already converted to another Policy.");
            }


            if (!string.IsNullOrWhiteSpace(request.RowVersion))
            {
                ApplyOriginalRowVersion(
                    quote,
                    request.RowVersion);
            }


            if (request.PolicyId == Guid.Empty ||
                request.PaymentId == Guid.Empty)
            {
                throw new BadRequestException(
                    "INVALID_CONVERSION_REFERENCE",
                    "Policy and Payment identifiers are required.");
            }


            if (Round(request.PaidAmount) !=
                quote.FinalAnnualPremium)
            {
                throw new ConflictException(
                    "PAYMENT_AMOUNT_MISMATCH",
                    "The paid amount does not match the Quote premium.");
            }


            await ValidateEligibilityAsync(
                quote,
                utcNow,
                cancellationToken);


            quote.Status =
                QuoteStatus.ConvertedToPolicy;
            quote.ConvertedPolicyId =
                request.PolicyId;
            quote.ConvertedAtUtc =
                NormalizeUtc(
                    request.PaymentSucceededAtUtc);
            quote.ConversionCorrelationId =
                request.CorrelationId;
            quote.PaymentDeadlineUtc = null;
            quote.UpdatedAtUtc = utcNow;


            await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _auditService.AuditAsync(
                        "QuoteConvertedToPolicy",
                        "Quote",
                        quote.Id.ToString(),
                        null,
                        JsonSerializer.Serialize(
                            new
                            {
                                quote.Status,
                                quote.ConvertedPolicyId,
                                request.PaymentId,
                                request.PaidAmount,
                                quote.ConvertedAtUtc
                            }),
                        token);


                    await _unitOfWork.SaveChangesAsync(token);
                },
                cancellationToken);


            return quote.ToConversionResultDto(
                wasAlreadyConverted: false);
        }


        private async Task<Quote> GetEligibleQuoteAsync(
            Guid quoteId,
            Guid customerId,
            decimal? paymentAmount,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            Quote quote =
                await _quoteRepository.GetDetailsAsync(
                    quoteId,
                    tracking: false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");


            if (quote.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "The Customer does not own the Quote.");
            }


            if (paymentAmount.HasValue &&
                Round(paymentAmount.Value) !=
                quote.FinalAnnualPremium)
            {
                throw new ConflictException(
                    "PAYMENT_AMOUNT_MISMATCH",
                    "Payment amount does not match the final annual premium.");
            }


            await ValidateEligibilityAsync(
                quote,
                utcNow,
                cancellationToken);


            return quote;
        }


        private async Task ValidateEligibilityAsync(
            Quote quote,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            if (quote.Status !=
                QuoteStatus.AdminApproved)
            {
                throw new ConflictException(
                    "QUOTE_NOT_ADMIN_APPROVED",
                    "The Quote is not approved for payment.");
            }


            if (!quote.CustomerAcceptedAtUtc.HasValue ||
                quote.AcceptedByCustomerId !=
                    quote.CustomerId ||
                !quote.AdminDecisionAtUtc.HasValue ||
                quote.CustomerAcceptedAtUtc <=
                    quote.AdminDecisionAtUtc)
            {
                throw new ConflictException(
                    "QUOTE_NOT_ACCEPTED",
                    "The Quote has not been accepted by the Customer after Admin approval.");
            }


            if (quote.AdminDecision !=
                    AdminDecision.Approved ||
                !quote.AdminDecisionAtUtc.HasValue)
            {
                throw new ConflictException(
                    "QUOTE_NOT_ADMIN_APPROVED",
                    "The Quote does not contain a valid Admin approval.");
            }


            if (quote.ExpiresAtUtc <= utcNow)
            {
                throw new ConflictException(
                    "QUOTE_EXPIRED",
                    "The Quote has expired.");
            }


            if (quote.PaymentDeadlineUtc.HasValue &&
                quote.PaymentDeadlineUtc.Value <= utcNow)
            {
                throw new ConflictException(
                    "PAYMENT_DEADLINE_EXPIRED",
                    "The payment deadline has expired.");
            }


            PropertyDetailsDto property =
                await _customerClient.GetPropertyAsync(
                    quote.PropertyId,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "PROPERTY_NOT_FOUND",
                    "Property was not found.");
            if (property.CustomerId != quote.CustomerId)
            {
                throw new ConflictException(
                    "PROPERTY_OWNERSHIP_CHANGED",
                    "The Property is no longer owned by the Quote Customer.");
            }


            if (property.IsArchived)
            {
                throw new ConflictException(
                    "PROPERTY_ARCHIVED",
                    "An archived Property cannot be used for payment.");
            }


            if (!MatchesProperty(quote, property))
            {
                throw new ConflictException(
                    "PROPERTY_SNAPSHOT_CHANGED",
                    "Material Property information changed after Quote generation.");
            }


            PolicyOverlapResult overlap =
                await _policyClient.CheckOverlapAsync(
                    new PolicyOverlapRequest(
                        quote.CustomerId,
                        quote.PropertyId,
                        quote.ProductType,
                        quote.Id),
                    cancellationToken);


            if (overlap.HasConflict)
            {
                throw new ConflictException(
                    overlap.ConflictCode ??
                    "PRODUCT_OVERLAP",
                    overlap.Message ??
                    "Product overlap validation failed.");
            }
        }


        private static bool MatchesProperty(
            Quote quote,
            PropertyDetailsDto property)
        {
            return quote.PropertyValue ==
                       Round(property.PropertyValue) &&
                   quote.ConstructionYear ==
                       property.ConstructionYear &&
                    string.Equals(
                    quote.OccupancyFrequency,
                    property.OccupancyFrequency.ToString(),
                    StringComparison.OrdinalIgnoreCase) &&
                   quote.HasSecuritySystem ==
                       property.HasSecuritySystem &&
                   quote.PreviousClaimsCount ==
                       property.PreviousClaimsCount &&
                   quote.HasDisasterExposure ==
                       property.DisasterExposure;
        }
        private void ApplyOriginalRowVersion(
            Quote quote,
            string rowVersion)
        {
            if (string.IsNullOrWhiteSpace(rowVersion))
            {
                throw new BadRequestException(
                    "INVALID_ROW_VERSION",
                    "RowVersion is required.");
            }


            byte[] expectedRowVersion;


            try
            {
                expectedRowVersion =
                    Convert.FromBase64String(rowVersion);
            }
            catch (FormatException)
            {
                throw new BadRequestException(
                    "INVALID_ROW_VERSION",
                    "RowVersion must be valid Base64.");
            }


            _quoteRepository.SetOriginalRowVersion(
                quote,
                expectedRowVersion);
        }




        private static DateTime NormalizeUtc(
            DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local =>
                    value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(
                    value,
                    DateTimeKind.Utc)
            };
        }


        private static decimal Round(decimal value)
        {
            return Math.Round(
                value,
                2,
                MidpointRounding.AwayFromZero);
        }


        public async Task<RenewalQuoteResultDto>
            CreateRenewalQuoteAsync(
                CreateRenewalQuoteRequest request,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            QuoteDetailsDto quote =
                await _quoteGenerationService
                    .GenerateRenewalAsync(
                        request.PolicyId,
                        request.CustomerId,
                        cancellationToken);


            return new RenewalQuoteResultDto(
                quote.Id,
                quote.QuoteNumber,
                quote.FinalAnnualPremium,
                quote.ExpiresAtUtc);
        }
    }
}
