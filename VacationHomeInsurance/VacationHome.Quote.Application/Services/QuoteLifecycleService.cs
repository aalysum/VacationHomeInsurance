using System.Text.Json;
using Microsoft.Extensions.Logging;
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
    public sealed class QuoteLifecycleService
        : IQuoteLifecycleService
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly ICustomerServiceClient
            _customerServiceClient;
        private readonly IPolicyServiceClient
            _policyServiceClient;
        private readonly ICurrentUserService
            _currentUserService;
        private readonly ICorrelationContext
            _correlationContext;
        private readonly IAuditService _auditService;
        private readonly INotificationService
            _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<QuoteLifecycleService>
            _logger;
        public QuoteLifecycleService(
            IQuoteRepository quoteRepository,
            ICustomerServiceClient customerServiceClient,
            IPolicyServiceClient policyServiceClient,
            ICurrentUserService currentUserService,
            ICorrelationContext correlationContext,
            IAuditService auditService,
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            ILogger<QuoteLifecycleService> logger)
        {
            _quoteRepository = quoteRepository;
            _customerServiceClient =
                customerServiceClient;
            _policyServiceClient =
                policyServiceClient;
            _currentUserService =
                currentUserService;
            _correlationContext =
                correlationContext;
            _auditService = auditService;
            _notificationService =
                notificationService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }


        public async Task<QuoteDetailsDto> AcceptQuoteAsync(
            Guid quoteId,
            AcceptQuoteRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            DateTime utcNow = DateTime.UtcNow;
            Guid customerId =
                await ResolveCustomerIdAsync(
                    cancellationToken);
            


            Quote quote =
                await _quoteRepository.GetDetailsAsync(
                    quoteId,
                    tracking: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");


            EnsureCustomerOwnsQuote(
                quote,
                customerId);


            if (quote.Status ==
                    QuoteStatus.AdminApproved &&
                quote.AcceptedByCustomerId ==
                    customerId &&
                quote.CustomerAcceptedAtUtc.HasValue &&
                quote.AdminDecisionAtUtc.HasValue &&
                quote.CustomerAcceptedAtUtc >
                    quote.AdminDecisionAtUtc)
            {
                return await ToDetailsWithPolicyWarningAsync(
                    quote,
                    utcNow,
                    cancellationToken);
            }


            ApplyOriginalRowVersion(
                quote,
                request.RowVersion);


            EnsureApprovedAndNotExpired(
                quote,
                utcNow);




            PropertyDetailsDto currentProperty =
                await _customerServiceClient
                    .GetPropertyAsync(
                        quote.PropertyId,
                        cancellationToken)
                ?? throw new NotFoundException(
                    "PROPERTY_NOT_FOUND",
                    "The Property associated with the Quote was not found.");


            if (currentProperty.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "The Property does not belong to the Customer.");
            }


            if (!MatchesMaterialPropertySnapshot(
                    quote,
                    currentProperty))
            {
                await InvalidateTrackedQuoteAsync(
                    quote,
                    "Material Property information changed after Quote generation.",
                    customerId.ToString(),
                    "Customer",
                    utcNow,
                    cancellationToken);


                throw new ConflictException(
                    "PROPERTY_SNAPSHOT_CHANGED",
                    "Material Property information changed. Generate a new Quote.");
            }


            string oldValues = JsonSerializer.Serialize(
                new
                {
                    quote.AcceptedByCustomerId,
                    quote.CustomerAcceptedAtUtc,
                    quote.AcceptanceCorrelationId
                });


            quote.AcceptedByCustomerId =
                customerId;


            quote.CustomerAcceptedAtUtc =
                utcNow;


            quote.AcceptanceCorrelationId =
                GetCorrelationId();


            quote.UpdatedAtUtc = utcNow;


            string newValues = JsonSerializer.Serialize(
                new
                {
                    quote.Status,
                    quote.AcceptedByCustomerId,
                    quote.CustomerAcceptedAtUtc,
                    quote.FinalAnnualPremium,
                    quote.AdminDecisionAtUtc,
                    quote.AcceptanceCorrelationId
                });


            await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _auditService.AuditAsync(
                        action: "CustomerAcceptedApprovedQuote",
                        entityType: "Quote",
                        entityId: quote.Id.ToString(),
                        oldValues: oldValues,
                        newValues: newValues,
                        cancellationToken: token);


                    await _unitOfWork.SaveChangesAsync(
                        token);
                },
                cancellationToken);


            return await ToDetailsWithPolicyWarningAsync(
                quote,
                utcNow,
                cancellationToken);
        }

        private async Task<QuoteDetailsDto>
            ToDetailsWithPolicyWarningAsync(
                Quote quote,
                DateTime utcNow,
                CancellationToken cancellationToken)
        {
            QuoteDetailsDto details = quote.ToDetailsDto(utcNow);
            try
            {
                PolicyOverlapResult overlap =
                    await _policyServiceClient.CheckOverlapAsync(
                        new PolicyOverlapRequest(
                            quote.CustomerId,
                            quote.PropertyId,
                            quote.ProductType,
                            quote.Id),
                        cancellationToken);

                if (overlap.HasConflict)
                {
                    string conflict =
                        string.IsNullOrWhiteSpace(overlap.Message)
                            ? "An existing policy conflicts with this protection."
                            : overlap.Message;
                    details.PolicyConversionWarning =
                        $"{conflict} You may still accept and submit this quote, " +
                        "but it cannot be approved or converted to a policy while " +
                        "the conflicting policy remains active.";
                }
            }
            catch (DependencyUnavailableException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Unable to check policy conversion eligibility for quote {QuoteId}.",
                    quote.Id);
            }

            return details;
        }


        public async Task<QuoteDetailsDto> WithdrawQuoteAsync(
            Guid quoteId,
            WithdrawQuoteRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            DateTime utcNow = DateTime.UtcNow;
            Guid customerId =
                await ResolveCustomerIdAsync(
                    cancellationToken);


            Quote quote =
                await _quoteRepository.GetDetailsAsync(
                    quoteId,
                    tracking: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");


            EnsureCustomerOwnsQuote(
                quote,
                customerId);


            if (quote.Status == QuoteStatus.Withdrawn)
            {
                return quote.ToDetailsDto(utcNow);
            }


            ApplyOriginalRowVersion(
                quote,
                request.RowVersion);


            if (quote.Status != QuoteStatus.Active)
            {
                throw new ConflictException(
                    "QUOTE_NOT_WITHDRAWABLE",
                    "Only an unused active Quote can be withdrawn.");
            }


            EnsureNotExpired(
                quote,
                utcNow);


            string oldValues = JsonSerializer.Serialize(
                new
                {
                    quote.Status,
                    quote.WithdrawnAtUtc,
                    quote.WithdrawalReason
                });


            quote.Status = QuoteStatus.Withdrawn;
            quote.WithdrawnAtUtc = utcNow;
            quote.WithdrawalReason =
                NormalizeOptional(request.Reason);
            quote.WithdrawalCorrelationId =
                GetCorrelationId();
            quote.UpdatedAtUtc = utcNow;
            quote.PaymentDeadlineUtc = null;


            string newValues = JsonSerializer.Serialize(
                new
                {
                    quote.Status,
                    quote.WithdrawnAtUtc,
                    quote.WithdrawalReason,
                    quote.WithdrawalCorrelationId
                });


            await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _auditService.AuditAsync(
                        action: "QuoteWithdrawn",
                        entityType: "Quote",
                        entityId: quote.Id.ToString(),
                        oldValues: oldValues,
                        newValues: newValues,
                        cancellationToken: token);


                    await _unitOfWork.SaveChangesAsync(
                        token);
                },
                cancellationToken);


            return quote.ToDetailsDto(utcNow);
        }


        public async Task<int> ExpireEligibleQuotesAsync(
            DateTime processingTimeUtc,
            CancellationToken cancellationToken)
        {
            DateTime utcNow =
                NormalizeUtc(processingTimeUtc);


            IReadOnlyCollection<Quote> eligibleQuotes =
                await _quoteRepository
                    .GetEligibleForExpiryAsync(
                        utcNow,
                        cancellationToken);


            if (eligibleQuotes.Count == 0)
            {
                return 0;
            }


            int expiredCount = 0;


            await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    foreach (Quote quote in eligibleQuotes)
                    {
                        if (!CanExpire(
                                quote,
                                utcNow))
                        {
                            continue;
                        }


                        string oldValues =
                            JsonSerializer.Serialize(
                                new
                                {
                                    quote.Status,
                                    quote.ExpiresAtUtc,
                                    quote.PaymentDeadlineUtc
                                });


                        quote.Status =
                            QuoteStatus.Expired;


                        quote.ExpiredAtUtc =
                            utcNow;


                        quote.PaymentDeadlineUtc =
                            null;


                        quote.UpdatedAtUtc =
                            utcNow;


                        string newValues =
                            JsonSerializer.Serialize(
                                new
                                {
                                    quote.Status,
                                    quote.ExpiredAtUtc
                                });


                        await _auditService.AuditAsync(
                            action: "QuoteExpired",
                            entityType: "Quote",
                            entityId:
                                quote.Id.ToString(),
                            oldValues: oldValues,
                            newValues: newValues,
                            cancellationToken: token);


                        expiredCount++;
                    }


                    if (expiredCount > 0)
                    {
                        await _unitOfWork
                            .SaveChangesAsync(token);
                    }
                },
                cancellationToken);


            return expiredCount;
        }




        public async Task<QuoteDetailsDto>
            InvalidateQuoteAsync(
                Guid quoteId,
                string reason,
                string actorId,
                string actorRole,
                CancellationToken cancellationToken)
        {
            DateTime utcNow = DateTime.UtcNow;


            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new BadRequestException(
                    "INVALID_INVALIDATION_REASON",
                    "Quote invalidation reason is required.");
            }


            if (string.IsNullOrWhiteSpace(actorId))
            {
                throw new BadRequestException(
                    "INVALID_ACTOR_ID",
                    "Invalidation actor identifier is required.");
            }


            if (string.IsNullOrWhiteSpace(actorRole))
            {
                throw new BadRequestException(
                    "INVALID_ACTOR_ROLE",
                    "Invalidation actor role is required.");
            }


            Quote quote =
                await _quoteRepository.GetDetailsAsync(
                    quoteId,
                    tracking: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");


            if (quote.Status ==
                QuoteStatus.Invalidated)
            {
                return quote.ToDetailsDto(utcNow);
            }


            await InvalidateTrackedQuoteAsync(
                quote,
                reason,
                actorId,
                actorRole,
                utcNow,
                cancellationToken);


            return quote.ToDetailsDto(utcNow);
        }


        private async Task InvalidateTrackedQuoteAsync(
            Quote quote,
            string reason,
            string actorId,
            string actorRole,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            if (!CanInvalidate(quote.Status))
            {
                throw new ConflictException(
                    "QUOTE_CANNOT_BE_INVALIDATED",
                    "The Quote cannot be invalidated in its current status.");
            }


            string normalizedReason =
                reason.Trim();


            string oldValues = JsonSerializer.Serialize(
                new
                {
                    quote.Status,
                    quote.InvalidatedAtUtc,
                    quote.InvalidationReason
                });


            quote.Status =
                QuoteStatus.Invalidated;


            quote.InvalidatedAtUtc =
                utcNow;


            quote.InvalidationReason =
                normalizedReason;


            quote.InvalidationCorrelationId =
                GetCorrelationId();


            quote.PaymentDeadlineUtc =
                null;


            quote.UpdatedAtUtc =
                utcNow;


            string newValues = JsonSerializer.Serialize(
                new
                {
                    quote.Status,
                    quote.InvalidatedAtUtc,
                    quote.InvalidationReason,
                    quote.InvalidationCorrelationId,
                    ActorId = actorId,
                    ActorRole = actorRole
                });


            await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _auditService.AuditAsync(
                        action: "QuoteInvalidated",
                        entityType: "Quote",
                        entityId: quote.Id.ToString(),
                        oldValues: oldValues,
                        newValues: newValues,
                        cancellationToken: token);


                    await _unitOfWork.SaveChangesAsync(
                        token);
                },
                cancellationToken);
        }


        private async Task<Guid>
            ResolveCustomerIdAsync(
                CancellationToken cancellationToken)
        {
            Guid authUserId =
                RequireCustomer();


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
                throw new ConflictException(
                    "CUSTOMER_ID_MISMATCH",
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
            if (!_currentUserService.IsAuthenticated ||
                !_currentUserService.UserId.HasValue)
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
            }


            if (!string.Equals(
                    _currentUserService.Role,
                    "Customer",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenException(
                    "Customer role is required.");
            }


            return _currentUserService.UserId.Value;
        }


        private static void EnsureCustomerOwnsQuote(
            Quote quote,
            Guid customerId)
        {
            if (quote.CustomerId != customerId)
            {
                /*
                 * NotFound avoids disclosing whether another
                 * Customer's Quote identifier exists.
                 */
                throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");
            }
        }


        private static void EnsureApprovedAndNotExpired(
            Quote quote,
            DateTime utcNow)
        {
            if (quote.Status != QuoteStatus.AdminApproved ||
                quote.AdminDecision != AdminDecision.Approved ||
                !quote.DecidedByAdminId.HasValue ||
                !quote.AdminDecisionAtUtc.HasValue)
            {
                throw new ConflictException(
                    "QUOTE_NOT_ADMIN_APPROVED",
                    "Only an Admin-approved Quote can be accepted.");
            }


            EnsureNotExpired(
                quote,
                utcNow);
        }


        private static void EnsureNotExpired(
            Quote quote,
            DateTime utcNow)
        {
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
                    "The approved payment deadline has expired.");
            }
        }


        private static bool
            MatchesMaterialPropertySnapshot(
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


        private static bool CanExpire(
            Quote quote,
            DateTime utcNow)
        {
            if (quote.Status is
                QuoteStatus.Expired or
                QuoteStatus.Invalidated or
                QuoteStatus.Withdrawn or
                QuoteStatus.AdminRejected or
                QuoteStatus.ConvertedToPolicy)
            {
                return false;
            }


            bool quoteValidityExpired =
                quote.ExpiresAtUtc <= utcNow;


            bool paymentDeadlineExpired =
                quote.Status ==
                    QuoteStatus.AdminApproved &&
                quote.PaymentDeadlineUtc.HasValue &&
                quote.PaymentDeadlineUtc.Value <=
                    utcNow;


            return quoteValidityExpired ||
                   paymentDeadlineExpired;
        }


        private static bool CanInvalidate(
            QuoteStatus status)
        {
            return status is
                QuoteStatus.Active or
                QuoteStatus.PendingAdminApproval or
                QuoteStatus.AdminApproved;
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
                    Convert.FromBase64String(
                        rowVersion);
            }
            catch (FormatException)
            {
                throw new BadRequestException(
                    "INVALID_ROW_VERSION",
                    "RowVersion must be a valid Base64 value.");
            }


            _quoteRepository.SetOriginalRowVersion(
                quote,
                expectedRowVersion);
        }


        private async Task TryNotifyAsync(
            Func<Task> notification,
            string errorMessage,
            Guid quoteId)
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
                    "{ErrorMessage} QuoteId: {QuoteId}",
                    errorMessage,
                    quoteId);
            }
        }


        private string GetCorrelationId()
        {
            return string.IsNullOrWhiteSpace(
                _correlationContext.CorrelationId)
                    ? Guid.NewGuid().ToString()
                    : _correlationContext.CorrelationId;
        }


        private static string? NormalizeOptional(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        private static DateTime NormalizeUtc(
            DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,


                DateTimeKind.Local =>
                    value.ToUniversalTime(),


                DateTimeKind.Unspecified =>
                    DateTime.SpecifyKind(
                        value,
                        DateTimeKind.Utc),


                _ => value
            };
        }


        private static decimal Round(decimal value)
        {
            return Math.Round(
                value,
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}
