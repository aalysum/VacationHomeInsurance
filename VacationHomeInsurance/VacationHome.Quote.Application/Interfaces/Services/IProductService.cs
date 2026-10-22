using VacationHome.ProductQuote.Application.DTOs.Pricing;
using VacationHome.ProductQuote.Application.DTOs.Products;


namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface IProductService
    {


        Task<PricingConfigurationDto>
            GetPricingAsync(
                CancellationToken cancellationToken);


        Task<IReadOnlyCollection<ProductDetailsDto>>
            GetProductsAsync(
                CancellationToken cancellationToken);


        Task<IReadOnlyCollection<ProductDetailsDto>>
            GetAdminProductsAsync(
                CancellationToken cancellationToken);


        Task<ProductDetailsDto> GetAdminProductAsync(
            Guid productId,
            CancellationToken cancellationToken);


        Task<ProductDetailsDto> GetProductAsync(
            Guid productId,
            CancellationToken cancellationToken);


        Task UpdateProductStatusAsync(
            Guid productId,
            UpdateProductStatusRequest request,
            CancellationToken cancellationToken);


        Task UpdateCoverageLimitAsync(
            Guid productId,
            Guid coverageId,
            UpdateCoverageLimitRequest request,
            CancellationToken cancellationToken);


        Task UpdatePricingAsync(
            UpdatePricingRequest request,
            CancellationToken cancellationToken);


        Task UpdateCoverageChargeAsync(
            Guid productId,
            Guid coverageId,
            UpdateCoverageChargeRequest request,
            CancellationToken cancellationToken);
    }
}
