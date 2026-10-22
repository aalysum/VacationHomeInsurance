using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.Interfaces.Persistence
{
    public interface IProductRepository
    {


        Task<IReadOnlyCollection<Product>> GetAllAsync(
            CancellationToken cancellationToken);
        Task<IReadOnlyCollection<Product>> GetActiveAsync(
            CancellationToken cancellationToken);


        Task<Product?> GetByIdWithCoveragesAsync(
            Guid productId,
            bool tracking,
            CancellationToken cancellationToken);


        Task<bool> ExistsAsync(
            Guid productId,
            CancellationToken cancellationToken);


        void Update(Product product);
        void SetOriginalRowVersion(
            Product product,
            byte[] rowVersion);


        Task<Product?> GetByTypeAsync(
            ProductType productType,
            CancellationToken cancellationToken);


    }
}
