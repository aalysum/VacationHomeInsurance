using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Interfaces.Persistence
{
    public interface IProductCoverageRepository
    {
        Task<List<ProductCoverage>> GetByProductIdAsync(
            Guid productId,
            CancellationToken cancellationToken);


        Task<ProductCoverage?> GetByProductAndCoverageAsync(
            Guid productId,
            Guid coverageId,
            bool tracking,
            CancellationToken cancellationToken);


        Task AddAsync(
            ProductCoverage mapping,
            CancellationToken cancellationToken);


        Task RemoveAsync(
            ProductCoverage mapping,
            CancellationToken cancellationToken);


        void Update(ProductCoverage mapping);


        void SetOriginalRowVersion(
            ProductCoverage mapping,
            byte[] rowVersion);
    }
}
