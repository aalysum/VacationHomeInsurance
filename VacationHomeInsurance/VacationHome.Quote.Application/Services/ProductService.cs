using System.Text.Json;
using VacationHome.ProductQuote.Application.DTOs.Pricing;
using VacationHome.ProductQuote.Application.DTOs.Products;
using VacationHome.ProductQuote.Application.Exceptions;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Application.Interfaces.Services;
using VacationHome.ProductQuote.Application.Services;
using VacationHome.ProductQuote.Application.Mappers;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.Services
{
    public sealed class ProductService : IProductService
    {
        private readonly IProductRepository
            _productRepository;


        private readonly IProductCoverageRepository
            _productCoverageRepository;


        private readonly IPricingConfigurationRepository
            _pricingRepository;


        private readonly IConfigurationCacheService
            _cache;


        private readonly IAuditService
            _auditService;


        private readonly ICurrentUserService
            _currentUser;


        private readonly IUnitOfWork
            _unitOfWork;


        public ProductService(
            IProductRepository productRepository,
            IProductCoverageRepository
                productCoverageRepository,
            IPricingConfigurationRepository
                pricingRepository,
            IConfigurationCacheService cache,
            IAuditService auditService,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository =
                productRepository;


            _productCoverageRepository =
                productCoverageRepository;


            _pricingRepository =
                pricingRepository;


            _cache =
                cache;


            _auditService =
                auditService;


            _currentUser =
                currentUser;


            _unitOfWork =
                unitOfWork;
        }


        public async Task<
            IReadOnlyCollection<ProductDetailsDto>>
            GetProductsAsync(
                CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Product> products =
                await _cache.GetActiveProductsAsync(
                    cancellationToken);


            return products
                .Select(
                    ProductMapper.ToDetailsDto)
                .ToArray();
        }


        public async Task<ProductDetailsDto>
            GetProductAsync(
                Guid productId,
                CancellationToken cancellationToken)
        {
            ValidateProductId(
                productId);


            Product product =
                await _cache.GetProductAsync(
                    productId,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "PRODUCT_NOT_FOUND",
                    "Product was not found.");


            return product.ToDetailsDto();
        }


        public async Task<
            IReadOnlyCollection<ProductDetailsDto>>
            GetAdminProductsAsync(
                CancellationToken cancellationToken)
        {
            RequireAdmin();


            IReadOnlyCollection<Product> products =
                await _productRepository.GetAllAsync(
                    cancellationToken);


            return products
                .Select(
                    ProductMapper.ToDetailsDto)
                .ToArray();
        }


        public async Task<ProductDetailsDto>
            GetAdminProductAsync(
                Guid productId,
                CancellationToken cancellationToken)
        {
            RequireAdmin();


            ValidateProductId(
                productId);


            Product product =
                await _productRepository
                    .GetByIdWithCoveragesAsync(
                        productId,
                        tracking: false,
                        cancellationToken)
                ?? throw new NotFoundException(
                    "PRODUCT_NOT_FOUND",
                    "Product was not found.");


            return product.ToDetailsDto();
        }


        public async Task UpdateProductStatusAsync(
            Guid productId,
            UpdateProductStatusRequest request,
            CancellationToken cancellationToken)
        {
            RequireAdmin();


            ArgumentNullException.ThrowIfNull(
                request);


            ValidateProductId(
                productId);


            Product product =
                await _productRepository
                    .GetByIdWithCoveragesAsync(
                        productId,
                        tracking: true,
                        cancellationToken)
                ?? throw new NotFoundException(
                    "PRODUCT_NOT_FOUND",
                    "Product was not found.");


            byte[] expectedRowVersion =
                ParseRowVersion(
                    request.RowVersion);


            _productRepository
                .SetOriginalRowVersion(
                    product,
                    expectedRowVersion);


            if (
                product.IsActive ==
                request.IsActive)
            {
                return;
            }


            if (
                request.IsActive &&
                !product.Coverages.Any(
                    mapping =>
                        mapping.IsActive &&
                        mapping.Coverage.IsActive &&
                        mapping.CoverageType ==
                        CoverageType.Mandatory))
            {
                throw new ConflictException(
                    "MANDATORY_COVERAGE_MISSING",
                    "The Product must have at least one active mandatory Coverage before activation.");
            }


            string oldValues =
                JsonSerializer.Serialize(
                    new
                    {
                        product.IsActive
                    });


            product.IsActive =
                request.IsActive;


            string newValues =
                JsonSerializer.Serialize(
                    new
                    {
                        product.IsActive
                    });


            _productRepository.Update(
                product);


            await _auditService.AuditAsync(
                request.IsActive
                    ? "ProductActivated"
                    : "ProductDeactivated",
                "Product",
                product.Id.ToString(),
                oldValues,
                newValues,
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);


            _cache.InvalidateProduct(
                product.Id);
        }


        public async Task UpdateCoverageChargeAsync(
            Guid productId,
            Guid coverageId,
            UpdateCoverageChargeRequest request,
            CancellationToken cancellationToken)
        {
            RequireAdmin();


            ArgumentNullException.ThrowIfNull(
                request);


            ValidateProductId(
                productId);


            ValidateCoverageId(
                coverageId);


            ProductCoverage mapping =
                await _productCoverageRepository
                    .GetByProductAndCoverageAsync(
                        productId,
                        coverageId,
                        tracking: true,
                        cancellationToken)
                ?? throw new NotFoundException(
                    "PRODUCT_COVERAGE_NOT_FOUND",
                    "The Product-Coverage mapping was not found.");


            byte[] expectedRowVersion =
                ParseRowVersion(
                    request.RowVersion);


            _productCoverageRepository
                .SetOriginalRowVersion(
                    mapping,
                    expectedRowVersion);


            if (request.CoverageCharge < 0)
            {
                throw new BadRequestException(
                    "INVALID_COVERAGE_CHARGE",
                    "Coverage charge cannot be negative.");
            }

            if (request.CoverageCharge > 9999999999999999.99m ||
                decimal.Round(
                    request.CoverageCharge,
                    2,
                    MidpointRounding.AwayFromZero) !=
                    request.CoverageCharge)
            {
                throw new BadRequestException(
                    "INVALID_COVERAGE_CHARGE_PRECISION",
                    "Annual coverage charge must be within the supported range and contain no more than two decimal places.");
            }


            if (
                mapping.CoverageCharge ==
                request.CoverageCharge)
            {
                return;
            }


            string oldValues =
                JsonSerializer.Serialize(
                    new
                    {
                        mapping.CoverageCharge
                    });


            mapping.CoverageCharge =
                request.CoverageCharge;


            string newValues =
                JsonSerializer.Serialize(
                    new
                    {
                        mapping.CoverageCharge
                    });


            _productCoverageRepository.Update(
                mapping);


            await _auditService.AuditAsync(
                "CoverageChargeUpdated",
                "ProductCoverage",
                mapping.Id.ToString(),
                oldValues,
                newValues,
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);


            _cache.InvalidateProduct(
                productId);


            _cache.InvalidateCoverages();
        }


        public async Task UpdateCoverageLimitAsync(
            Guid productId,
            Guid coverageId,
            UpdateCoverageLimitRequest request,
            CancellationToken cancellationToken)
        {
            RequireAdmin();


            ArgumentNullException.ThrowIfNull(
                request);


            ValidateProductId(
                productId);


            ValidateCoverageId(
                coverageId);


            ProductCoverage mapping =
                await _productCoverageRepository
                    .GetByProductAndCoverageAsync(
                        productId,
                        coverageId,
                        tracking: true,
                        cancellationToken)
                ?? throw new NotFoundException(
                    "PRODUCT_COVERAGE_NOT_FOUND",
                    "The Product-Coverage mapping was not found.");


            byte[] expectedRowVersion =
                ParseRowVersion(
                    request.RowVersion);


            _productCoverageRepository
                .SetOriginalRowVersion(
                    mapping,
                    expectedRowVersion);


            if (
                request.CoverageLimitPercentage <= 0 ||
                request.CoverageLimitPercentage > 100 ||
                decimal.Round(
                    request.CoverageLimitPercentage,
                    4,
                    MidpointRounding.AwayFromZero) !=
                    request.CoverageLimitPercentage)
            {
                throw new BadRequestException(
                    "INVALID_COVERAGE_LIMIT_PERCENTAGE",
                    "Coverage-limit percentage must be greater than zero, no more than 100, and contain no more than four decimal places.");
            }


            if (
                mapping.CoverageLimitPercentage ==
                request.CoverageLimitPercentage)
            {
                return;
            }


            string oldValues =
                JsonSerializer.Serialize(
                    new
                    {
                        mapping.ProductId,
                        mapping.CoverageId,
                        mapping.CoverageLimitPercentage
                    });


            mapping.CoverageLimitPercentage =
                request.CoverageLimitPercentage;


            string newValues =
                JsonSerializer.Serialize(
                    new
                    {
                        mapping.ProductId,
                        mapping.CoverageId,
                        mapping.CoverageLimitPercentage
                    });


            _productCoverageRepository.Update(
                mapping);


            await _auditService.AuditAsync(
                "CoverageLimitUpdated",
                "ProductCoverage",
                mapping.Id.ToString(),
                oldValues,
                newValues,
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);


            _cache.InvalidateProduct(
                productId);


            _cache.InvalidateCoverages();
        }


        public async Task UpdatePricingAsync(
            UpdatePricingRequest request,
            CancellationToken cancellationToken)
        {
            RequireAdmin();


            ArgumentNullException.ThrowIfNull(
                request);


            PricingConfiguration pricing =
                await _pricingRepository.GetActiveAsync(
                    tracking: true,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "PRICING_CONFIGURATION_NOT_FOUND",
                    "Active pricing configuration was not found.");


            byte[] expectedRowVersion =
                ParseRowVersion(
                    request.RowVersion);


            _pricingRepository
                .SetOriginalRowVersion(
                    pricing,
                    expectedRowVersion);


            ValidatePricing(
                request);


            if (
                pricing.BaseRate ==
                    request.BaseRate &&
                pricing.LowRiskMultiplier ==
                    request.LowRiskMultiplier &&
                pricing.MediumRiskMultiplier ==
                    request.MediumRiskMultiplier &&
                pricing.HighRiskMultiplier ==
                    request.HighRiskMultiplier)
            {
                return;
            }


            string oldValues =
                JsonSerializer.Serialize(
                    new
                    {
                        pricing.BaseRate,
                        pricing.LowRiskMultiplier,
                        pricing.MediumRiskMultiplier,
                        pricing.HighRiskMultiplier
                    });


            pricing.BaseRate =
                request.BaseRate;


            pricing.LowRiskMultiplier =
                request.LowRiskMultiplier;


            pricing.MediumRiskMultiplier =
                request.MediumRiskMultiplier;


            pricing.HighRiskMultiplier =
                request.HighRiskMultiplier;


            pricing.UpdatedAtUtc =
                DateTime.UtcNow;


            string newValues =
                JsonSerializer.Serialize(
                    new
                    {
                        pricing.BaseRate,
                        pricing.LowRiskMultiplier,
                        pricing.MediumRiskMultiplier,
                        pricing.HighRiskMultiplier,
                        pricing.UpdatedAtUtc
                    });


            _pricingRepository.Update(
                pricing);


            await _auditService.AuditAsync(
                "PricingConfigurationUpdated",
                "PricingConfiguration",
                pricing.Id.ToString(),
                oldValues,
                newValues,
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);


            _cache.InvalidatePricing();
        }


        private void RequireAdmin()
        {
            if (
                !_currentUser.IsAuthenticated ||
                !_currentUser.UserId.HasValue)
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
            }


            if (
                !string.Equals(
                    _currentUser.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenException(
                    "Admin role is required.");
            }
        }


        private static void ValidateProductId(
            Guid productId)
        {
            if (productId == Guid.Empty)
            {
                throw new BadRequestException(
                    "INVALID_PRODUCT_ID",
                    "Product identifier is required.");
            }
        }


        private static void ValidateCoverageId(
            Guid coverageId)
        {
            if (coverageId == Guid.Empty)
            {
                throw new BadRequestException(
                    "INVALID_COVERAGE_ID",
                    "Coverage identifier is required.");
            }
        }


        private static void ValidatePricing(
            UpdatePricingRequest request)
        {
            if (request.BaseRate <
                    PricingAssumptions.MinimumBaseRate ||
                request.BaseRate >
                    PricingAssumptions.MaximumBaseRate ||
                decimal.Round(
                    request.BaseRate,
                    6,
                    MidpointRounding.AwayFromZero) !=
                    request.BaseRate)
            {
                throw new BadRequestException(
                    "INVALID_BASE_RATE",
                    "Base rate must be between 0.0001 and 0.02 of the insured property value and contain no more than six decimal places.");
            }


            if (
                request.LowRiskMultiplier <
                    PricingAssumptions.MinimumRiskMultiplier ||
                request.MediumRiskMultiplier <
                    PricingAssumptions.MinimumRiskMultiplier ||
                request.HighRiskMultiplier <
                    PricingAssumptions.MinimumRiskMultiplier ||
                request.LowRiskMultiplier >
                    PricingAssumptions.MaximumRiskMultiplier ||
                request.MediumRiskMultiplier >
                    PricingAssumptions.MaximumRiskMultiplier ||
                request.HighRiskMultiplier >
                    PricingAssumptions.MaximumRiskMultiplier ||
                request.LowRiskMultiplier >
                    request.MediumRiskMultiplier ||
                request.MediumRiskMultiplier >
                    request.HighRiskMultiplier)
            {
                throw new BadRequestException(
                    "INVALID_RISK_MULTIPLIER",
                    "Risk multipliers must be between 0.5 and 3 and increase from low to high risk.");
            }

            if (decimal.Round(
                    request.LowRiskMultiplier,
                    6,
                    MidpointRounding.AwayFromZero) !=
                    request.LowRiskMultiplier ||
                decimal.Round(
                    request.MediumRiskMultiplier,
                    6,
                    MidpointRounding.AwayFromZero) !=
                    request.MediumRiskMultiplier ||
                decimal.Round(
                    request.HighRiskMultiplier,
                    6,
                    MidpointRounding.AwayFromZero) !=
                    request.HighRiskMultiplier)
            {
                throw new BadRequestException(
                    "INVALID_RISK_MULTIPLIER_PRECISION",
                    "Risk multipliers must contain no more than six decimal places.");
            }
        }






        public async Task<PricingConfigurationDto>
            GetPricingAsync(
                CancellationToken cancellationToken)
        {
            RequireAdmin();


            PricingConfiguration pricing =
                await _pricingRepository.GetActiveAsync(
                    tracking: false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "PRICING_CONFIGURATION_NOT_FOUND",
                    "Active pricing configuration was not found.");


            return pricing.ToDto();
        }
        private static byte[] ParseRowVersion(
    string rowVersion)
        {
            if (string.IsNullOrWhiteSpace(
                    rowVersion))
            {
                throw new BadRequestException(
                    "INVALID_ROW_VERSION",
                    "RowVersion is required.");
            }


            try
            {
                byte[] parsedRowVersion =
                    Convert.FromBase64String(
                        rowVersion);


                if (parsedRowVersion.Length == 0)
                {
                    throw new BadRequestException(
                        "INVALID_ROW_VERSION",
                        "RowVersion must not be empty.");
                }


                return parsedRowVersion;
            }
            catch (FormatException)
            {
                throw new BadRequestException(
                    "INVALID_ROW_VERSION",
                    "RowVersion must be a valid Base64 value.");
            }
        }
    }
}
