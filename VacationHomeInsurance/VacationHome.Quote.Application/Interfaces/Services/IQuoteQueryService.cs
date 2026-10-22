using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Admin;
using VacationHome.ProductQuote.Application.DTOs.Quotes;


namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface IQuoteQueryService
    {
        Task<QuoteDetailsDto> GetMyQuoteAsync(
            Guid quoteId,
            CancellationToken cancellationToken);


        Task<PagedResult<QuoteSummaryDto>>
            SearchMyQuotesAsync(
                QuoteSearchRequest request,
                CancellationToken cancellationToken);


        Task<QuoteDetailsDto> GetAdminQuoteAsync(
            Guid quoteId,
            CancellationToken cancellationToken);


        Task<PagedResult<QuoteSummaryDto>>
            SearchAdminQuotesAsync(
                AdminQuoteSearchRequest request,
                CancellationToken cancellationToken);
    }
}
