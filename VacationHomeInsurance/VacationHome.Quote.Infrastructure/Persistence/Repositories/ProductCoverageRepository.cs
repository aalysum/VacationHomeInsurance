using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Repositories
{
    public sealed class ProductCoverageRepository :
        IProductCoverageRepository
    {
        private readonly ProductQuoteDbContext _dbContext;


        public ProductCoverageRepository(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<ProductCoverage>>
            GetByProductIdAsync(
                Guid productId,
                CancellationToken cancellationToken)
        {
            return await _dbContext
                .Set<ProductCoverage>()
                .AsNoTracking()
                .Include(mapping => mapping.Coverage)
                .Where(mapping =>
                    mapping.ProductId == productId)
                .OrderBy(mapping =>
                    mapping.DisplayOrder)
                .ThenBy(mapping =>
                    mapping.Coverage.Name)
                .ToListAsync(cancellationToken);
        }




        public void SetOriginalRowVersion(
            ProductCoverage mapping,
            byte[] rowVersion)
        {
            ArgumentNullException.ThrowIfNull(mapping);
            ArgumentNullException.ThrowIfNull(rowVersion);


            _dbContext.Entry(mapping)
                .Property(x => x.RowVersion)
                .OriginalValue = rowVersion;
        }


        public async Task AddAsync(
            ProductCoverage mapping,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(mapping);


            await _dbContext
                .Set<ProductCoverage>()
                .AddAsync(
                    mapping,
                    cancellationToken);
        }


        public Task RemoveAsync(
            ProductCoverage mapping,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(mapping);


            cancellationToken.ThrowIfCancellationRequested();


            _dbContext.Set<ProductCoverage>().Remove(mapping);


            return Task.CompletedTask;
        }


        public async Task<ProductCoverage?>
            GetByProductAndCoverageAsync(
                Guid productId,
                Guid coverageId,
                bool tracking,
                CancellationToken cancellationToken)
        {
            IQueryable<ProductCoverage> query =
                _dbContext.Set<ProductCoverage>()
                    .Include(x => x.Coverage);


            if (!tracking)
            {
                query = query.AsNoTracking();
            }


            return await query.SingleOrDefaultAsync(
                x =>
                    x.ProductId == productId &&
                    x.CoverageId == coverageId,
                cancellationToken);
        }


        public void Update(ProductCoverage mapping)
        {
            ArgumentNullException.ThrowIfNull(mapping);


            _dbContext.Set<ProductCoverage>().Update(mapping);
        }
    }
}
