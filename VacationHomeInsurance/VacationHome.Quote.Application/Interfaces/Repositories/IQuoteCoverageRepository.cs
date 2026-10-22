using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Interfaces.Repositories
{
    public interface IQuoteCoverageRepository
    {
        Task AddRangeAsync(
            IReadOnlyCollection<QuoteCoverage> coverages,
            CancellationToken cancellationToken);


        Task<IReadOnlyCollection<QuoteCoverage>>
            GetByQuoteIdAsync(
                Guid quoteId,
                CancellationToken cancellationToken);
    }
}
