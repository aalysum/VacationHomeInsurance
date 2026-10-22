using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Interfaces.Persistence
{
    public interface IQuoteApprovalHistoryRepository
    {
        Task AddAsync(
            QuoteApprovalHistory history,
            CancellationToken cancellationToken);
    }
}
