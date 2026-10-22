using VacationHome.ProductQuote.Application.DTOs.Admin;
using VacationHome.ProductQuote.Application.DTOs.Quotes;


namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface IQuoteAdminService
    {
        Task<QuoteDetailsDto> ApproveAsync(
            Guid quoteId,
            ApproveQuoteRequest request,
            CancellationToken cancellationToken);


        Task<QuoteDetailsDto> RejectAsync(
            Guid quoteId,
            AdminRejectionRequest request,
            CancellationToken cancellationToken);
    }
}
