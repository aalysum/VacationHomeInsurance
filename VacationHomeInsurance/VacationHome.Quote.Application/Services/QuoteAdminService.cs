using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VacationHome.ProductQuote.Application.Configuration;
using VacationHome.ProductQuote.Application.DTOs.Admin;
using VacationHome.ProductQuote.Application.DTOs.External;
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
    public sealed class QuoteAdminService
        : IQuoteAdminService
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteApprovalHistoryRepository
            _historyRepository;
        private readonly ICustomerServiceClient _customerClient;
        private readonly IPolicyServiceClient _policyClient;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notifications;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICorrelationContext _correlation;
        private readonly ProductQuoteOptions _options;
        private readonly ILogger<QuoteAdminService> _logger;


        public QuoteAdminService(
            ICurrentUserService currentUser,
            IQuoteRepository quoteRepository,
            IQuoteApprovalHistoryRepository historyRepository,
            ICustomerServiceClient customerClient,
            IPolicyServiceClient policyClient,
            IAuditService auditService,
            INotificationService notifications,
            IUnitOfWork unitOfWork,
            ICorrelationContext correlation,
            IOptions<ProductQuoteOptions> options,
            ILogger<QuoteAdminService> logger)
        {
            _currentUser = currentUser;
            _quoteRepository = quoteRepository;
            _historyRepository = historyRepository;
            _customerClient = customerClient;
            _policyClient = policyClient;
            _auditService = auditService;
            _notifications = notifications;
            _unitOfWork = unitOfWork;
            _correlation = correlation;
            _options = options.Value;
            _logger = logger;
        }


        public async Task<QuoteDetailsDto> ApproveAsync(
            Guid quoteId,
            ApproveQuoteRequest request,
            CancellationToken cancellationToken)
        {
            DateTime utcNow = DateTime.UtcNow;
            Guid adminId = RequireAdmin();


            Quote quote = await LoadPendingAsync(
                quoteId,
                request.RowVersion,
                utcNow,
                cancellationToken);


            await ValidateCurrentEligibilityAsync(
                quote,
                cancellationToken);

            decimal originalAnnualPremium =
                quote.FinalAnnualPremium;
            decimal originalMonthlyEquivalent =
                quote.EstimatedMonthlyEquivalent;
            string? priceAdjustmentReason =
                NormalizeOptional(request.PriceAdjustmentReason);
            bool priceChanged =
                request.RevisedAnnualPremium.HasValue &&
                request.RevisedAnnualPremium.Value !=
                    originalAnnualPremium;

            if (request.RevisedAnnualPremium.HasValue)
            {
                decimal revisedAnnualPremium =
                    request.RevisedAnnualPremium.Value;
                if (revisedAnnualPremium <= 0 ||
                    revisedAnnualPremium > 9999999999999999.99m ||
                    decimal.Round(
                        revisedAnnualPremium,
                        2,
                        MidpointRounding.AwayFromZero) != revisedAnnualPremium)
                {
                    throw new BadRequestException(
                        "INVALID_REVISED_PREMIUM",
                        "Revised annual premium must be positive and contain no more than two decimal places.");
                }

                if (priceChanged)
                {
                    if (priceAdjustmentReason is null ||
                        priceAdjustmentReason.Length < 5 ||
                        priceAdjustmentReason.Length > 500)
                    {
                        throw new BadRequestException(
                            "INVALID_PRICE_ADJUSTMENT_REASON",
                            "A price adjustment reason of 5 to 500 characters is required.");
                    }

                    quote.FinalAnnualPremium =
                        revisedAnnualPremium;
                    quote.EstimatedMonthlyEquivalent =
                        PremiumCalculator.CalculateMonthlyEquivalent(
                            revisedAnnualPremium);
                }
            }


            DateTime deadline =
                utcNow.AddDays(
                    _options.ApprovedPaymentPeriodDays);


            if (deadline > quote.ExpiresAtUtc)
            {
                deadline = quote.ExpiresAtUtc;
            }


            string oldValues = JsonSerializer.Serialize(
                new
                {
                    quote.Status,
                    quote.AdminDecision,
                    FinalAnnualPremium = originalAnnualPremium,
                    EstimatedMonthlyEquivalent =
                        originalMonthlyEquivalent,
                    quote.AcceptedByCustomerId,
                    quote.CustomerAcceptedAtUtc
                });


            quote.Status = QuoteStatus.AdminApproved;
            quote.AcceptedByCustomerId = null;
            quote.CustomerAcceptedAtUtc = null;
            quote.AcceptanceCorrelationId = null;
            quote.DecidedByAdminId = adminId;
            quote.AdminDecision = AdminDecision.Approved;
            quote.AdminDecisionAtUtc = utcNow;
            quote.ApprovalRemarks =
                NormalizeOptional(request.Remarks);
            quote.RejectionReason = null;
            quote.AdminDecisionCorrelationId =
                _correlation.CorrelationId;
            quote.PaymentDeadlineUtc = deadline;
            quote.UpdatedAtUtc = utcNow;


            var history = new QuoteApprovalHistory
            {
                Id = Guid.NewGuid(),
                QuoteId = quote.Id,
                AdminId = adminId,
                Decision = AdminDecision.Approved,
                Remarks = priceChanged
                    ? priceAdjustmentReason
                    : quote.ApprovalRemarks,
                CorrelationId =
                    _correlation.CorrelationId,
                DecisionAtUtc = utcNow,
                CreatedAtUtc = utcNow
            };


            await PersistDecisionAsync(
                quote,
                history,
                "QuoteApproved",
                oldValues,
                utcNow,
                cancellationToken,
                priceChanged ? priceAdjustmentReason : null);


            await TryNotifyAsync(
                () => _notifications.NotifyQuoteApprovedAsync(
                    quote.CustomerId,
                    quote.Id,
                    deadline,
                    cancellationToken));


            return quote.ToDetailsDto(utcNow);
        }


        public async Task<QuoteDetailsDto> RejectAsync(
            Guid quoteId,
            AdminRejectionRequest request,
            CancellationToken cancellationToken)
        {
            DateTime utcNow = DateTime.UtcNow;
            Guid adminId = RequireAdmin();


            string reason =
                request.Reason?.Trim() ?? string.Empty;


            if (reason.Length < 20 ||
                reason.Length > 1000)
            {
                throw new BadRequestException(
                    "INVALID_REJECTION_REASON",
                    "Rejection reason must contain between 20 and 1000 characters.");
            }


            Quote quote = await LoadPendingAsync(
                quoteId,
                request.RowVersion,
                utcNow,
                cancellationToken);


            string oldValues = JsonSerializer.Serialize(
                new
                {
                    quote.Status,
                    quote.AdminDecision
                });


            quote.Status = QuoteStatus.AdminRejected;
            quote.DecidedByAdminId = adminId;
            quote.AdminDecision = AdminDecision.Rejected;
            quote.AdminDecisionAtUtc = utcNow;
            quote.ApprovalRemarks = null;
            quote.RejectionReason = reason;
            quote.AdminDecisionCorrelationId =
                _correlation.CorrelationId;
            quote.PaymentDeadlineUtc = null;
            quote.UpdatedAtUtc = utcNow;


            var history = new QuoteApprovalHistory
            {
                Id = Guid.NewGuid(),
                QuoteId = quote.Id,
                AdminId = adminId,
                Decision = AdminDecision.Rejected,
                RejectionReason = reason,
                CorrelationId =
                    _correlation.CorrelationId,
                DecisionAtUtc = utcNow,
                CreatedAtUtc = utcNow
            };


            await PersistDecisionAsync(
                quote,
                history,
                "QuoteRejected",
                oldValues,
                utcNow,
                cancellationToken);


            await TryNotifyAsync(
                () => _notifications.NotifyQuoteRejectedAsync(
                    quote.CustomerId,
                    quote.Id,
                    reason,
                    cancellationToken));


            return quote.ToDetailsDto(utcNow);
        }


        private async Task<Quote> LoadPendingAsync(
            Guid quoteId,
            string rowVersion,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            Quote quote =
                await _quoteRepository.GetDetailsAsync(
                    quoteId,
                    tracking: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");


            ApplyOriginalRowVersion(
                quote,
                rowVersion);


            if (quote.Status is
                QuoteStatus.AdminApproved or
                QuoteStatus.AdminRejected)
            {
                throw new ConflictException(
                    "QUOTE_ALREADY_DECIDED",
                    "The Quote already has a final Admin decision.");
            }


            if (quote.Status !=
                QuoteStatus.PendingAdminApproval)
            {
                throw new ConflictException(
                    "INVALID_QUOTE_STATUS",
                    "The Quote is not pending Admin approval.");
            }


            if (quote.ExpiresAtUtc <= utcNow)
            {
                throw new ConflictException(
                    "QUOTE_EXPIRED",
                    "The Quote has expired.");
            }


            return quote;
        }


        private async Task ValidateCurrentEligibilityAsync(
            Quote quote,
            CancellationToken cancellationToken)
        {
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
                    "An archived Property cannot be used for Quote approval.");
            }


            if (!MatchesProperty(quote, property))
            {
                throw new ConflictException(
                    "PROPERTY_SNAPSHOT_CHANGED",
                    "Material Property information changed after Quote generation.");
            }


            IReadOnlyCollection<ProductType> inFlightProductTypes =
                await _quoteRepository
                    .GetInFlightProductTypesAsync(
                        customerId: quote.CustomerId,
                        propertyId: quote.PropertyId,
                        excludedQuoteId: quote.Id,
                        asOfUtc: DateTime.UtcNow,
                        cancellationToken: cancellationToken);


            ValidateInFlightProductConflicts(
                quote.ProductType,
                inFlightProductTypes);


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


        private static void ValidateInFlightProductConflicts(
            ProductType requestedProductType,
            IReadOnlyCollection<ProductType> existingProductTypes)
        {
            bool hasComprehensiveProtection =
                existingProductTypes.Contains(
                    ProductType.ComprehensiveProtection);
            bool hasOtherProtection =
                existingProductTypes.Any(
                    productType =>
                        productType !=
                        ProductType.ComprehensiveProtection);


            if (
                requestedProductType ==
                    ProductType.ComprehensiveProtection &&
                hasOtherProtection
            )
            {
                throw new ConflictException(
                    "COMPREHENSIVE_POLICY_CONFLICT",
                    "Property Protection and Natural Disaster Protection may coexist, but neither may coexist with Comprehensive Protection on the same property.");
            }


            if (
                requestedProductType !=
                    ProductType.ComprehensiveProtection &&
                hasComprehensiveProtection
            )
            {
                throw new ConflictException(
                    "COMPREHENSIVE_POLICY_EXISTS",
                    "Property Protection and Natural Disaster Protection may coexist, but neither may coexist with Comprehensive Protection on the same property.");
            }


            if (existingProductTypes.Contains(requestedProductType))
            {
                throw new ConflictException(
                    "DUPLICATE_PRODUCT_QUOTE",
                    "A quote for this protection is already pending or approved for this property.");
            }
        }


        private async Task PersistDecisionAsync(
            Quote quote,
            QuoteApprovalHistory history,
            string action,
            string oldValues,
            DateTime utcNow,
            CancellationToken cancellationToken,
            string? priceAdjustmentReason = null)
        {
            await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _historyRepository.AddAsync(
                        history,
                        token);


                    await _auditService.AuditAsync(
                        action,
                        "Quote",
                        quote.Id.ToString(),
                        oldValues,
                        JsonSerializer.Serialize(
                            new
                            {
                                quote.Status,
                                quote.AdminDecision,
                                quote.AdminDecisionAtUtc,
                                quote.FinalAnnualPremium,
                                quote.EstimatedMonthlyEquivalent,
                                quote.ApprovalRemarks,
                                quote.RejectionReason,
                                quote.PaymentDeadlineUtc,
                                PriceAdjustmentReason =
                                    priceAdjustmentReason
                            }),
                        token);


                    await _unitOfWork.SaveChangesAsync(token);
                },
                cancellationToken);
        }


        private Guid RequireAdmin()
        {
            if (!_currentUser.IsAuthenticated ||
                !_currentUser.UserId.HasValue)
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
            }


            if (!string.Equals(
                    _currentUser.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenException(
                    "Admin role is required.");
            }


            return _currentUser.UserId.Value;
        }


        private async Task TryNotifyAsync(
            Func<Task> notification)
        {
            try
            {
                await notification();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Admin Quote notification failed.");
            }
        }


        private static bool MatchesProperty(
            Quote quote,
            PropertyDetailsDto property)
        {
            return quote.PropertyValue ==
                       Math.Round(
                           property.PropertyValue,
                           2,
                           MidpointRounding.AwayFromZero) &&
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
        private static string? NormalizeOptional(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
