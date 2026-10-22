namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task NotifyQuoteGeneratedAsync(
            Guid customerId,
            Guid quoteId,
            string quoteNumber,
            CancellationToken cancellationToken);


        Task NotifyQuoteSubmittedAsync(
            Guid customerId,
            Guid quoteId,
            CancellationToken cancellationToken);


        Task NotifyAdminQuotePendingAsync(
            Guid quoteId,
            CancellationToken cancellationToken);


        Task NotifyQuoteApprovedAsync(
            Guid customerId,
            Guid quoteId,
            DateTime paymentDeadlineUtc,
            CancellationToken cancellationToken);


        Task NotifyQuoteRejectedAsync(
            Guid customerId,
            Guid quoteId,
            string reason,
            CancellationToken cancellationToken);
    }
}
