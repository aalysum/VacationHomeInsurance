using VacationHome.ProductQuote.Application.DTOs.Quotes;


namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface IQuoteLifecycleService
    {
        Task<QuoteDetailsDto> AcceptQuoteAsync(
            Guid quoteId,
            AcceptQuoteRequest request,
            CancellationToken cancellationToken);


        Task<QuoteDetailsDto> WithdrawQuoteAsync(
            Guid quoteId,
            WithdrawQuoteRequest request,
            CancellationToken cancellationToken);


        Task<int> ExpireEligibleQuotesAsync(
            DateTime processingTimeUtc,
            CancellationToken cancellationToken);


        Task<QuoteDetailsDto> InvalidateQuoteAsync(
            Guid quoteId,
            string reason,
            string actorId,
            string actorRole,
            CancellationToken cancellationToken);
    }
}
