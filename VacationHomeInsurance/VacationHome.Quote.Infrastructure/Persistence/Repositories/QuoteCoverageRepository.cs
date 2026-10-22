using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Application.Interfaces.Repositories;
using VacationHome.ProductQuote.Domain.Entities;




namespace VacationHome.ProductQuote.Infrastructure.Persistence.Repositories
{
    public sealed class QuoteCoverageRepository
        : IQuoteCoverageRepository
    {
        private readonly ProductQuoteDbContext _context;


        public QuoteCoverageRepository(
            ProductQuoteDbContext context)
        {
            _context = context;
        }


        public async Task AddRangeAsync(
            IReadOnlyCollection<QuoteCoverage> coverages,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(coverages);


            if (coverages.Count == 0)
            {
                return;
            }


            await _context.QuoteCoverages.AddRangeAsync(
                coverages,
                cancellationToken);
        }


        public async Task<IReadOnlyCollection<QuoteCoverage>>
            GetByQuoteIdAsync(
                Guid quoteId,
                CancellationToken cancellationToken)
        {
            return await _context.QuoteCoverages
                .AsNoTracking()
                .Where(coverage =>
                    coverage.QuoteId == quoteId)
                .OrderBy(coverage =>
                    coverage.DisplayOrder)
                .ThenBy(coverage =>
                    coverage.CoverageName)
                .ToArrayAsync(cancellationToken);
        }
    }
}
