using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Infrastructure.Persistence;


namespace VacationHome.ProductQuote.Infrastructure.Persistence.Repositories
{
    public sealed class CoverageRepository :
        ICoverageRepository
    {
        private readonly ProductQuoteDbContext _dbContext;


        public CoverageRepository(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<Coverage?> GetByIdAsync(
            Guid coverageId,
            bool tracking,
            CancellationToken cancellationToken)
        {
            IQueryable<Coverage> query =
                _dbContext.Coverages;


            if (!tracking)
            {
                query = query.AsNoTracking();
            }


            return await query.SingleOrDefaultAsync(
                coverage => coverage.Id == coverageId,
                cancellationToken);
        }


        public async Task<IReadOnlyCollection<Coverage>>
            GetAllAsync(
                CancellationToken cancellationToken)
        {
            return await _dbContext.Coverages
                .AsNoTracking()
                .OrderBy(coverage => coverage.Name)
                .ToArrayAsync(cancellationToken);
        }


        public void Update(Coverage coverage)
        {
            ArgumentNullException.ThrowIfNull(coverage);


            _dbContext.Coverages.Update(coverage);
        }
    }
}
