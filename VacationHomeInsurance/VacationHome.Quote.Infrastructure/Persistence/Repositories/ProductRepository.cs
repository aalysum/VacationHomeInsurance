using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Repositories
{
    public sealed class ProductRepository :
        IProductRepository
    {
        private readonly ProductQuoteDbContext _dbContext;


        public ProductRepository(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IReadOnlyCollection<Product>>
            GetAllAsync(
                CancellationToken cancellationToken)
        {
            return await _dbContext
                .Set<Product>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(product =>
                    product.Coverages)
                .ThenInclude(mapping =>
                    mapping.Coverage)
                .OrderByDescending(product =>
                    product.IsActive)
                .ThenBy(product =>
                    product.Name)
                .ToArrayAsync(
                    cancellationToken);
        }


        public async Task<IReadOnlyCollection<Product>>
            GetActiveAsync(
                CancellationToken cancellationToken)
        {
            return await _dbContext
                .Set<Product>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(product =>
                    product.Coverages)
                .ThenInclude(mapping =>
                    mapping.Coverage)
                .Where(product =>
                    product.IsActive)
                .OrderBy(product =>
                    product.Name)
                .ToArrayAsync(cancellationToken);
        }


        public async Task<Product?>
            GetByIdWithCoveragesAsync(
                Guid productId,
                bool tracking,
                CancellationToken cancellationToken)
        {
            IQueryable<Product> query =
                _dbContext
                    .Set<Product>()
                    .AsSplitQuery()
                    .Include(product =>
                        product.Coverages)
                    .ThenInclude(mapping =>
                        mapping.Coverage);


            if (!tracking)
            {
                query = query.AsNoTracking();
            }


            return await query.SingleOrDefaultAsync(
                product =>
                    product.Id == productId,
                cancellationToken);
        }


        public async Task<bool> ExistsAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            return await _dbContext
                .Set<Product>()
                .AsNoTracking()
                .AnyAsync(
                    product =>
                        product.Id == productId,
                    cancellationToken);
        }


        public void SetOriginalRowVersion(
            Product product,
            byte[] rowVersion)
        {
            ArgumentNullException.ThrowIfNull(product);
            ArgumentNullException.ThrowIfNull(rowVersion);


            _dbContext.Entry(product)
                .Property(x => x.RowVersion)
                .OriginalValue = rowVersion;
        }


        public void Update(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);


            _dbContext
                .Set<Product>()
                .Update(product);
        }
        public async Task<Product?> GetByTypeAsync(
    ProductType productType,
    CancellationToken cancellationToken)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(product => product.Coverages)
                    .ThenInclude(mapping => mapping.Coverage)
                .SingleOrDefaultAsync(
                    product =>
                        product.ProductType == productType,
                    cancellationToken);
        }


    }
}
