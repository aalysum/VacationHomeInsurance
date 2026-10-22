using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface IConfigurationCacheService
    {
        Task<IReadOnlyCollection<Product>>
            GetActiveProductsAsync(
                CancellationToken cancellationToken);


        Task<Product?> GetProductAsync(
            Guid productId,
            CancellationToken cancellationToken);


        Task<PricingConfiguration?>
            GetActivePricingAsync(
                CancellationToken cancellationToken);


        Task<RiskConfiguration?>
            GetActiveRiskConfigurationAsync(
                CancellationToken cancellationToken);


        Task<Product?> GetProductByTypeAsync(
            ProductType productType,
            CancellationToken cancellationToken);
        void InvalidateProducts();


        void InvalidateProduct(Guid productId);


        void InvalidateCoverages();


        void InvalidatePricing();


        void InvalidateRiskConfiguration();
    }
}
