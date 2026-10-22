using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.Configuration;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Application.Interfaces.Services;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.Services
{
    public sealed class ConfigurationCacheService
        : IConfigurationCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IProductRepository _productRepository;
        private readonly IPricingConfigurationRepository
            _pricingRepository;
        private readonly IRiskConfigurationRepository
            _riskConfigurationRepository;
        private readonly ProductQuoteOptions _options;


        public ConfigurationCacheService(
            IMemoryCache cache,
            IProductRepository productRepository,
            IPricingConfigurationRepository pricingRepository,
            IRiskConfigurationRepository
                riskConfigurationRepository,
            IOptions<ProductQuoteOptions> options)
        {
            _cache = cache;
            _productRepository = productRepository;
            _pricingRepository = pricingRepository;
            _riskConfigurationRepository =
                riskConfigurationRepository;
            _options = options.Value;
        }


        public async Task<IReadOnlyCollection<Product>>
            GetActiveProductsAsync(
                CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Product>? cachedProducts =
                await _cache.GetOrCreateAsync(
                    ProductQuoteCacheKeys.ActiveProducts,
                    async entry =>
                    {
                        ConfigureCacheEntry(entry);


                        return await _productRepository
                            .GetActiveAsync(
                                cancellationToken);
                    });


            return cachedProducts
                ?? Array.Empty<Product>();
        }


        public async Task<Product?> GetProductAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            if (productId == Guid.Empty)
            {
                return null;
            }


            string cacheKey =
                ProductQuoteCacheKeys.Product(productId);


            return await _cache.GetOrCreateAsync(
                cacheKey,
                async entry =>
                {
                    ConfigureCacheEntry(entry);


                    return await _productRepository
                        .GetByIdWithCoveragesAsync(
                            productId,
                            tracking: false,
                            cancellationToken);
                });
        }


        public async Task<PricingConfiguration?>
            GetActivePricingAsync(
                CancellationToken cancellationToken)
        {
            return await _cache.GetOrCreateAsync(
                ProductQuoteCacheKeys.ActivePricing,
                async entry =>
                {
                    ConfigureCacheEntry(entry);


                    return await _pricingRepository
                        .GetActiveAsync(
                            tracking: false,
                            cancellationToken);
                });
        }


        public async Task<RiskConfiguration?>
            GetActiveRiskConfigurationAsync(
                CancellationToken cancellationToken)
        {
            return await _cache.GetOrCreateAsync(
                ProductQuoteCacheKeys
                    .ActiveRiskConfiguration,
                async entry =>
                {
                    ConfigureCacheEntry(entry);


                    return await _riskConfigurationRepository
                        .GetActiveAsync(
                            tracking: false,
                            cancellationToken);
                });
        }


        public void InvalidateProducts()
        {
            _cache.Remove(
                ProductQuoteCacheKeys.ActiveProducts);


            _cache.Remove(
                ProductQuoteCacheKeys.AllProducts);
        }


        public void InvalidateProduct(Guid productId)
        {
            if (productId == Guid.Empty)
            {
                return;
            }


            _cache.Remove(
                ProductQuoteCacheKeys.Product(productId));


            _cache.Remove(
                ProductQuoteCacheKeys.ProductCoverages(
                    productId));


            InvalidateProducts();
        }


        public void InvalidateCoverages()
        {
            _cache.Remove(
                ProductQuoteCacheKeys.Coverages);


            /*
             * A Coverage activation or deactivation can change
             * the Product catalogue returned to callers.
             */
            InvalidateProducts();
        }


        public void InvalidatePricing()
        {
            _cache.Remove(
                ProductQuoteCacheKeys.ActivePricing);
        }


        public void InvalidateRiskConfiguration()
        {
            _cache.Remove(
                ProductQuoteCacheKeys
                    .ActiveRiskConfiguration);
        }


        private void ConfigureCacheEntry(
            ICacheEntry entry)
        {
            int durationMinutes =
                _options.CacheDurationMinutes > 0
                    ? _options.CacheDurationMinutes
                    : 30;


            entry.AbsoluteExpirationRelativeToNow =
                TimeSpan.FromMinutes(durationMinutes);


            entry.Priority = CacheItemPriority.Normal;


            entry.Size = 1;
        }


        public async Task<Product?> GetProductByTypeAsync(
            ProductType productType,
            CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(productType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(productType),
                    productType,
                    "Product type is invalid.");
            }


            string cacheKey =
                $"product:type:{(int)productType}";


            if (_cache.TryGetValue(
                    cacheKey,
                    out Product? cachedProduct))
            {
                return cachedProduct;
            }


            Product? product =
                await _productRepository.GetByTypeAsync(
                    productType,
                    cancellationToken);


            if (product is null)
            {
                return null;
            }


            _cache.Set(
                cacheKey,
                product,
                TimeSpan.FromMinutes(10));


            return product;
        }
    }
}
