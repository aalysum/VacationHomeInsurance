using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VacationHome.ProductQuote.Application.Configuration;
using VacationHome.ProductQuote.Application.DTOs.External;
using VacationHome.ProductQuote.Application.DTOs.Quotes;
using VacationHome.ProductQuote.Application.Exceptions;
using VacationHome.ProductQuote.Application.Interfaces.External;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Application.Interfaces.Services;
using VacationHome.ProductQuote.Application.Mappers;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;
using VacationHome.ProductQuote.Domain.ValueObjects;


namespace VacationHome.ProductQuote.Application.Services
{
    public sealed class QuoteGenerationService
        : IQuoteGenerationService
    {
        private readonly ICurrentUserService _currentUser;
        private readonly ICustomerServiceClient _customerServiceClient;
        private readonly IPolicyServiceClient _policyClient;
        private readonly IConfigurationCacheService _cache;
        private readonly IQuoteRepository _quoteRepository;
        private readonly ISequenceRepository _sequenceRepository;
        private readonly IPremiumCalculator _premiumCalculator;
        private readonly IRiskCalculator _riskCalculator;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notifications;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ProductQuoteOptions _options;
        private readonly ILogger<QuoteGenerationService> _logger;


        public QuoteGenerationService(
            ICurrentUserService currentUser,
            ICustomerServiceClient customerClient,
            IPolicyServiceClient policyClient,
            IConfigurationCacheService cache,
            IQuoteRepository quoteRepository,
            ISequenceRepository sequenceRepository,
            IPremiumCalculator premiumCalculator,
            IRiskCalculator riskCalculator,
            IAuditService auditService,
            INotificationService notifications,
            IUnitOfWork unitOfWork,
            IOptions<ProductQuoteOptions> options,
            ILogger<QuoteGenerationService> logger)
        {
            _currentUser = currentUser;
            _customerServiceClient = customerClient;
            _policyClient = policyClient;
            _cache = cache;
            _quoteRepository = quoteRepository;
            _sequenceRepository = sequenceRepository;
            _premiumCalculator = premiumCalculator;
            _riskCalculator = riskCalculator;
            _auditService = auditService;
            _notifications = notifications;
            _unitOfWork = unitOfWork;
            _options = options.Value;
            _logger = logger;
        }


        public async Task<QuoteDetailsDto> GenerateAsync(
           GenerateQuoteRequest request,
           CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            DateTime utcNow =
                DateTime.UtcNow;


            Guid authUserId =
                RequireCustomer();


            Guid customerId =
                await EnsureCustomerIsActiveAsync(
                    authUserId,
                    cancellationToken);


            PropertyDetailsDto property =
                await _customerServiceClient
                    .GetPropertyAsync(
                        request.PropertyId,
                        cancellationToken)
                ?? throw new NotFoundException(
                    "PROPERTY_NOT_FOUND",
                    "Property was not found.");


            ValidateProperty(
                property,
                customerId,
                utcNow);


            Product product =
                await _cache.GetProductAsync(
                    request.ProductId,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "PRODUCT_NOT_FOUND",
                    "Product was not found.");


            return await GenerateInternalAsync(
                customerId,
                property,
                product,
                request.SelectedOptionalCoverageIds,
                excludedQuoteId: null,
                isRenewal: false,
                cancellationToken);
        }

        public async Task<QuoteDetailsDto>
            GenerateRenewalAsync(
                Guid policyId,
                Guid customerId,
                CancellationToken cancellationToken)
        {
            if (policyId == Guid.Empty)
            {
                throw new BadRequestException(
                    "INVALID_POLICY_ID",
                    "Policy identifier is required.");
            }


            if (customerId == Guid.Empty)
            {
                throw new BadRequestException(
                    "INVALID_CUSTOMER_ID",
                    "Customer identifier is required.");
            }


            InternalPolicyDto policy =
                await _policyClient.GetPolicyAsync(
                    policyId,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "POLICY_NOT_FOUND",
                    "Policy was not found.");


            if (policy.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "The Policy does not belong to the Customer.");
            }


            await EnsureCustomerIsActiveAsync(
                customerId,
                cancellationToken);


            PropertyDetailsDto property =
                await _customerServiceClient.GetPropertyAsync(
                    policy.PropertyId,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "PROPERTY_NOT_FOUND",
                    "Property was not found.");


            ValidateProperty(
                property,
                customerId,
                DateTime.UtcNow);


            Product product =
                await _cache.GetProductByTypeAsync(
                    policy.ProductType,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "PRODUCT_NOT_FOUND",
                    "The Product for the Policy was not found.");


            /*
             * The original policy's quote is excluded from the
             * overlap check so that the renewal does not conflict
             * with its own source policy.
             *
             * The empty optional coverage collection means the
             * renewed quote uses the Product's current mandatory
             * coverages. Optional coverages can be selected again
             * later if your renewal requirements allow it.
             */
            return await GenerateInternalAsync(
                customerId,
                property,
                product,
                Array.Empty<Guid>(),
                excludedQuoteId: policy.QuoteId,
                isRenewal: true,
                cancellationToken);
        }


        private async Task<QuoteDetailsDto>
            GenerateInternalAsync(
                Guid customerId,
                PropertyDetailsDto property,
                Product product,
                IReadOnlyCollection<Guid> optionalCoverageIds,
                Guid? excludedQuoteId,
                bool isRenewal,
                CancellationToken cancellationToken)
        {
            DateTime utcNow = DateTime.UtcNow;


            if (!product.IsActive)
            {
                throw new ConflictException(
                    "PRODUCT_INACTIVE",
                    "The selected Product is inactive.");
            }


            IReadOnlyCollection<ProductCoverage>
                purchasedCoverages =
                    ResolveCoverages(
                        product,
                        optionalCoverageIds);

            decimal insuredPropertyValue =
                Round(property.PropertyValue);


            PricingConfiguration pricing =
                await _cache.GetActivePricingAsync(
                    cancellationToken)
                ?? throw new ConflictException(
                    "PRICING_CONFIGURATION_NOT_FOUND",
                    "Active pricing configuration was not found.");


            RiskConfiguration riskConfiguration =
                await _cache
                    .GetActiveRiskConfigurationAsync(
                        cancellationToken)
                ?? throw new ConflictException(
                    "RISK_CONFIGURATION_NOT_FOUND",
                    "Active risk configuration was not found.");


            Guid[] normalizedOptionalIds =
                optionalCoverageIds
                    .Distinct()
                    .OrderBy(id => id)
                    .ToArray();


            Quote? duplicate =
                await _quoteRepository
                    .FindIdenticalActiveQuoteAsync(
                        customerId,
                        property.PropertyId,
                        product.Id,
                        insuredPropertyValue,
                        property.ConstructionYear,
                        property.OccupancyFrequency
                            .ToString(),
                        property.HasSecuritySystem,
                        property.PreviousClaimsCount,
                        property.DisasterExposure,
                        pricing.Id,
                        normalizedOptionalIds,
                        cancellationToken);


            if (duplicate is not null)
            {
                QuoteDetailsDto duplicateDetails =
                    duplicate.ToDetailsDto(utcNow);
                duplicateDetails.PolicyConversionWarning =
                    await GetPolicyConversionWarningAsync(
                        customerId,
                        property.PropertyId,
                        product.ProductType,
                        duplicate.Id,
                        cancellationToken);
                return duplicateDetails;
            }


            RiskSnapshot risk =
                _riskCalculator.Calculate(
                    property.ConstructionYear,
                    insuredPropertyValue,
                    property.PreviousClaimsCount,
                    property.DisasterExposure,
                    property.HasSecuritySystem,
                    property.OccupancyFrequency
                        .ToString(),
                    riskConfiguration);


            decimal mandatoryCharges =
                purchasedCoverages
                    .Where(mapping =>
                        mapping.CoverageType ==
                        CoverageType.Mandatory)
                    .Sum(mapping =>
                        mapping.CoverageCharge);


            decimal optionalCharges =
                purchasedCoverages
                    .Where(mapping =>
                        mapping.CoverageType ==
                        CoverageType.Optional)
                    .Sum(mapping =>
                        mapping.CoverageCharge);


            PremiumSnapshot premium =
                _premiumCalculator.Calculate(
                    insuredPropertyValue,
                    risk.RiskLevel,
                    pricing.BaseRate,
                    pricing.LowRiskMultiplier,
                    pricing.MediumRiskMultiplier,
                    pricing.HighRiskMultiplier,
                    mandatoryCharges,
                    optionalCharges);


            long sequence =
                await _sequenceRepository
                    .GetNextQuoteSequenceAsync(
                        utcNow.Year,
                        cancellationToken);


            if (sequence > 99999)
            {
                throw new ConflictException(
                    "QUOTE_SEQUENCE_EXHAUSTED",
                    "The Quote sequence for the current year is exhausted.");
            }


            Quote quote =
                CreateQuote(
                    customerId,
                    property,
                    product,
                    pricing,
                    riskConfiguration,
                    risk,
                    premium,
                    $"QT-{utcNow.Year}-{sequence:D5}",
                    utcNow);

            foreach (ProductCoverage mapping
                     in purchasedCoverages)
            {
                quote.Coverages.Add(
                    new QuoteCoverage
                    {
                        Id = Guid.NewGuid(),


                        QuoteId = quote.Id,


                        CoverageId =
                            mapping.CoverageId,


                        ProductCoverageId =
                            mapping.Id,


                        CoverageName =
                            mapping.Coverage.Name,


                        CoverageDescription =
                            mapping.Coverage.Description,


                        CoverageType =
                            mapping.CoverageType,


                        CoverageCharge =
                            Round(
                                mapping.CoverageCharge),


                        CoverageLimitPercentage =
                            mapping
                                .CoverageLimitPercentage,


                        CoverageLimitAmount =
                            CoverageLimitCalculator.Calculate(
                                insuredPropertyValue,
                                mapping.CoverageLimitPercentage),


                        DisplayOrder =
                            mapping.DisplayOrder,


                        CreatedAtUtc =
                            utcNow
                    });
            }


            string auditAction =
                isRenewal
                    ? "RenewalQuoteGenerated"
                    : "QuoteGenerated";


            await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _quoteRepository.AddAsync(
                        quote,
                        token);


                    await _auditService.AuditAsync(
                        auditAction,
                        "Quote",
                        quote.Id.ToString(),
                        null,
                        JsonSerializer.Serialize(
                            new
                            {
                                quote.QuoteNumber,
                                quote.CustomerId,
                                quote.PropertyId,
                                quote.ProductId,
                                quote.ProductType,
                                quote.RiskScore,
                                quote.FinalAnnualPremium,
                                IsRenewal = isRenewal,
                                ExcludedQuoteId =
                                    excludedQuoteId
                            }),
                        token);


                    await _unitOfWork.SaveChangesAsync(
                        token);
                },
                cancellationToken);


            try
            {
                await _notifications
                    .NotifyQuoteGeneratedAsync(
                        customerId,
                        quote.Id,
                        quote.QuoteNumber,
                        cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Quote-generated notification failed for Quote {QuoteId}.",
                    quote.Id);
            }

            try
            {
                await _notifications
                    .NotifyAdminQuotePendingAsync(
                        quote.Id,
                        cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Admin pending-Quote notification failed for Quote {QuoteId}.",
                    quote.Id);
            }


            QuoteDetailsDto details = quote.ToDetailsDto(utcNow);
            details.PolicyConversionWarning =
                await GetPolicyConversionWarningAsync(
                    customerId,
                    property.PropertyId,
                    product.ProductType,
                    quote.Id,
                    cancellationToken);
            return details;
        }

        private async Task<string?> GetPolicyConversionWarningAsync(
            Guid customerId,
            Guid propertyId,
            ProductType productType,
            Guid quoteId,
            CancellationToken cancellationToken)
        {
            try
            {
                PolicyOverlapResult overlap =
                    await _policyClient.CheckOverlapAsync(
                        new PolicyOverlapRequest(
                            customerId,
                            propertyId,
                            productType,
                            quoteId),
                        cancellationToken);

                if (!overlap.HasConflict)
                {
                    return null;
                }

                string conflict = string.IsNullOrWhiteSpace(overlap.Message)
                    ? "An existing policy conflicts with this protection."
                    : overlap.Message;

                return $"{conflict} You may still accept and submit this quote, " +
                       "but it cannot be approved or converted to a policy while " +
                       "the conflicting policy remains active.";
            }
            catch (DependencyUnavailableException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Unable to check policy conversion eligibility while generating quote for property {PropertyId}.",
                    propertyId);
                return null;
            }
        }


        private async Task<Guid>
            EnsureCustomerIsActiveAsync(
                Guid authUserId,
                CancellationToken cancellationToken)
        {
            CustomerStatusDto customerStatus =
                await _customerServiceClient
                    .GetCustomerStatusAsync(
                        authUserId,
                        cancellationToken);


            if (
                !Guid.TryParse(
                    customerStatus.AuthUserId,
                    out Guid returnedAuthUserId) ||
                returnedAuthUserId != authUserId)
            {
                throw new ForbiddenException(
                    "Customer Service returned a different authentication user identifier.");
            }


            if (!customerStatus.IsActive)
            {
                throw new ForbiddenException(
                    "The Customer account is inactive.");
            }


            return customerStatus.CustomerId;
        }


        private Guid RequireCustomer()
        {
            if (!_currentUser.IsAuthenticated ||
                !_currentUser.UserId.HasValue)
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
            }


            if (!string.Equals(
                    _currentUser.Role,
                    "Customer",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenException(
                    "Customer role is required.");
            }


            return _currentUser.UserId.Value;
        }


        private static void ValidateProperty(
            PropertyDetailsDto property,
            Guid customerId,
            DateTime utcNow)
        {
            if (property.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "The Property does not belong to the Customer.");
            }


            if (property.IsArchived)
            {
                throw new ConflictException(
                    "PROPERTY_ARCHIVED",
                    "An archived Property cannot be quoted.");
            }


            if (property.PropertyValue <= 0 ||
                property.ConstructionYear > utcNow.Year ||
                property.PreviousClaimsCount < 0)
            {
                throw new BadRequestException(
                    "INVALID_PROPERTY_RISK_INPUTS",
                    "Property risk inputs are invalid.");
            }
        }


        private static IReadOnlyCollection<ProductCoverage>
            ResolveCoverages(
                Product product,
                IReadOnlyCollection<Guid> optionalIds)
        {
            Guid[] requestedIds =
                optionalIds.ToArray();


            if (requestedIds.Length !=
                requestedIds.Distinct().Count())
            {
                throw new BadRequestException(
                    "DUPLICATE_COVERAGE_SELECTION",
                    "Optional Coverage IDs cannot be repeated.");
            }


            ProductCoverage[] mandatory =
                product.Coverages
                    .Where(mapping =>
                        mapping.IsActive &&
                        mapping.Coverage.IsActive &&
                        mapping.CoverageType ==
                        CoverageType.Mandatory)
                    .ToArray();


            if (mandatory.Length == 0)
            {
                throw new ConflictException(
                    "MANDATORY_COVERAGE_MISSING",
                    "The Product has no active mandatory Coverage.");
            }


            var selected =
                new List<ProductCoverage>(
                    mandatory);


            foreach (Guid coverageId
                     in requestedIds.OrderBy(id => id))
            {
                ProductCoverage? mapping =
                    product.Coverages
                        .SingleOrDefault(item =>
                            item.CoverageId ==
                            coverageId);


                if (mapping is null)
                {
                    throw new BadRequestException(
                        "COVERAGE_NOT_AVAILABLE_FOR_PRODUCT",
                        "A selected Coverage is not available for the Product.");
                }


                if (mapping.CoverageType ==
                    CoverageType.Mandatory)
                {
                    throw new BadRequestException(
                        "MANDATORY_COVERAGE_CANNOT_BE_OPTIONAL",
                        "Mandatory Coverage cannot be submitted as optional.");
                }


                if (!mapping.IsActive ||
                    !mapping.Coverage.IsActive)
                {
                    throw new BadRequestException(
                        "OPTIONAL_COVERAGE_INACTIVE",
                        "A selected optional Coverage is inactive.");
                }


                selected.Add(mapping);
            }


            return selected
                .OrderBy(mapping =>
                    mapping.DisplayOrder)
                .ToArray();
        }


        private Quote CreateQuote(
            Guid customerId,
            PropertyDetailsDto property,
            Product product,
            PricingConfiguration pricing,
            RiskConfiguration riskConfiguration,
            RiskSnapshot risk,
            PremiumSnapshot premium,
            string quoteNumber,
            DateTime utcNow)
        {
            return new Quote
            {
                Id = Guid.NewGuid(),


                QuoteNumber = quoteNumber,


                CustomerId = customerId,


                PropertyId =
                    property.PropertyId,


                ProductId =
                    product.Id,


                Status =
                    QuoteStatus.PendingAdminApproval,


                GeneratedAtUtc =
                    utcNow,


                ExpiresAtUtc =
                    utcNow.AddDays(
                        _options.QuoteValidityDays),


                ProductType =
                    product.ProductType,


                ProductName =
                    product.Name,


                ProductDescription =
                    product.Description,


                PropertyName =
                    property.PropertyName,


                PropertyType =
                    property.PropertyType.ToString(),


                AddressLine1 =
                    property.AddressLine1,


                AddressLine2 =
                    property.AddressLine2,


                City =
                    property.City,


                State =
                    property.State,


                PostalCode =
                    property.PostalCode,


                Country =
                    property.Country,


                PropertyValue =
                    Round(property.PropertyValue),


                ConstructionYear =
                    property.ConstructionYear,


                PropertyAge =
                    risk.PropertyAge,


                OccupancyFrequency =
                    property.OccupancyFrequency
                        .ToString(),


                HasSecuritySystem =
                    property.HasSecuritySystem,


                PreviousClaimsCount =
                    property.PreviousClaimsCount,


                HasDisasterExposure =
                    property.DisasterExposure,


                RiskConfigurationId =
                    riskConfiguration.Id,


                MediumPropertyValueThreshold =
                    riskConfiguration
                        .MediumPropertyValueThreshold,


                HighPropertyValueThreshold =
                    riskConfiguration
                        .HighPropertyValueThreshold,


                PropertyAgePoints =
                    risk.PropertyAgePoints,


                PreviousClaimsPoints =
                    risk.PreviousClaimsPoints,


                DisasterExposurePoints =
                    risk.DisasterExposurePoints,


                SecuritySystemPoints =
                    risk.SecuritySystemPoints,


                OccupancyPoints =
                    risk.OccupancyPoints,


                PropertyValuePoints =
                    risk.PropertyValuePoints,


                RiskScore =
                    risk.TotalRiskScore,


                RiskLevel =
                    risk.RiskLevel,


                RiskExplanation =
                    risk.Explanation,


                PricingConfigurationId =
                    pricing.Id,


                BaseRate =
                    premium.BaseRate,


                RiskMultiplier =
                    premium.RiskMultiplier,


                BasePremium =
                    premium.BasePremium,


                RiskAdjustedPremium =
                    premium.RiskAdjustedPremium,


                RiskAdjustmentAmount =
                    premium.RiskAdjustmentAmount,


                MandatoryCoverageCharges =
                    premium.MandatoryCoverageCharges,


                OptionalCoverageCharges =
                    premium.OptionalCoverageCharges,


                FinalAnnualPremium =
                    premium.FinalAnnualPremium,


                EstimatedMonthlyEquivalent =
                    premium.EstimatedMonthlyEquivalent,


                PaymentFrequency =
                    "Annual",


                CreatedAtUtc =
                    utcNow
            };
        }


        private static decimal Round(
            decimal value)
        {
            return Math.Round(
                value,
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}
