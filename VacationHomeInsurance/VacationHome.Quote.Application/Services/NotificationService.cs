using Microsoft.Extensions.Logging;
using VacationHome.ProductQuote.Application
    .Interfaces.Services;


namespace VacationHome.ProductQuote.Application.Services
{
    public sealed class NotificationService
        : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly ICorrelationContext _correlationContext;


        public NotificationService(
            ILogger<NotificationService> logger,
            ICorrelationContext correlationContext)
        {
            _logger = logger;
            _correlationContext = correlationContext;
        }


        public Task NotifyQuoteGeneratedAsync(
            Guid customerId,
            Guid quoteId,
            string quoteNumber,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();


            ValidateCustomerId(customerId);
            ValidateQuoteId(quoteId);


            if (string.IsNullOrWhiteSpace(quoteNumber))
            {
                throw new ArgumentException(
                    "Quote number is required.",
                    nameof(quoteNumber));
            }


            _logger.LogInformation(
                "Quote generated notification. " +
                "CustomerId: {CustomerId}, " +
                "QuoteId: {QuoteId}, " +
                "QuoteNumber: {QuoteNumber}, " +
                "CorrelationId: {CorrelationId}",
                customerId,
                quoteId,
                quoteNumber.Trim(),
                GetCorrelationId());


            return Task.CompletedTask;
        }


        public Task NotifyQuoteSubmittedAsync(
            Guid customerId,
            Guid quoteId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();


            ValidateCustomerId(customerId);
            ValidateQuoteId(quoteId);


            _logger.LogInformation(
                "Quote submitted for Admin approval notification. " +
                "CustomerId: {CustomerId}, " +
                "QuoteId: {QuoteId}, " +
                "CorrelationId: {CorrelationId}",
                customerId,
                quoteId,
                GetCorrelationId());


            return Task.CompletedTask;
        }


        public Task NotifyAdminQuotePendingAsync(
            Guid quoteId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();


            ValidateQuoteId(quoteId);


            _logger.LogInformation(
                "Admin pending Quote notification. " +
                "QuoteId: {QuoteId}, " +
                "CorrelationId: {CorrelationId}",
                quoteId,
                GetCorrelationId());


            return Task.CompletedTask;
        }


        public Task NotifyQuoteApprovedAsync(
            Guid customerId,
            Guid quoteId,
            DateTime paymentDeadlineUtc,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();


            ValidateCustomerId(customerId);
            ValidateQuoteId(quoteId);


            DateTime normalizedDeadline =
                NormalizeUtc(paymentDeadlineUtc);


            _logger.LogInformation(
                "Quote approved notification. " +
                "CustomerId: {CustomerId}, " +
                "QuoteId: {QuoteId}, " +
                "PaymentDeadlineUtc: {PaymentDeadlineUtc}, " +
                "CorrelationId: {CorrelationId}",
                customerId,
                quoteId,
                normalizedDeadline,
                GetCorrelationId());


            return Task.CompletedTask;
        }


        public Task NotifyQuoteRejectedAsync(
            Guid customerId,
            Guid quoteId,
            string reason,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();


            ValidateCustomerId(customerId);
            ValidateQuoteId(quoteId);


            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException(
                    "Quote rejection reason is required.",
                    nameof(reason));
            }


            _logger.LogInformation(
                "Quote rejected notification. " +
                "CustomerId: {CustomerId}, " +
                "QuoteId: {QuoteId}, " +
                "Reason: {Reason}, " +
                "CorrelationId: {CorrelationId}",
                customerId,
                quoteId,
                reason.Trim(),
                GetCorrelationId());


            return Task.CompletedTask;
        }


        private string GetCorrelationId()
        {
            return string.IsNullOrWhiteSpace(
                _correlationContext.CorrelationId)
                    ? "NotAvailable"
                    : _correlationContext.CorrelationId;
        }


        private static void ValidateCustomerId(
            Guid customerId)
        {
            if (customerId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Customer identifier is required.",
                    nameof(customerId));
            }
        }


        private static void ValidateQuoteId(
            Guid quoteId)
        {
            if (quoteId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quote identifier is required.",
                    nameof(quoteId));
            }
        }


        private static DateTime NormalizeUtc(DateTime value)
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
    }
}
