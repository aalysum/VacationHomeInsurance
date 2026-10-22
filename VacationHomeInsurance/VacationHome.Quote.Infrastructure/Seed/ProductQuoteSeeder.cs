using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Domain.Constants;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Infrastructure.Persistence
{
    public sealed class ProductQuoteDatabaseSeeder
    {
        private readonly ProductQuoteDbContext _dbContext;


        public ProductQuoteDatabaseSeeder(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task SeedAsync(
            CancellationToken cancellationToken = default)
        {
            await SeedCoveragesAsync(cancellationToken);
            await SeedProductsAsync(cancellationToken);
            await SeedProductCoveragesAsync(cancellationToken);
            await SeedPricingConfigurationAsync(cancellationToken);
            await SeedRiskConfigurationAsync(cancellationToken);


            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }


        private async Task SeedCoveragesAsync(
            CancellationToken cancellationToken)
        {
            if (await _dbContext.Coverages.AnyAsync(
                    cancellationToken))
            {
                return;
            }


            Coverage[] coverages =
            {
                new()
                {
                    Id = SeedIds.DwellingCoverageId,
                    Name = CoverageConstants.DwellingCoverage,
                    Description =
                        "Protects the insured dwelling against covered physical damage.",
                    IsActive = true
                },
                new()
                {
                    Id = SeedIds.OtherStructuresCoverageId,
                    Name =
                        CoverageConstants.OtherStructuresCoverage,
                    Description =
                        "Protects detached structures located on the insured property.",
                    IsActive = true
                },
                new()
                {
                    Id = SeedIds.PersonalPropertyCoverageId,
                    Name =
                        CoverageConstants.PersonalPropertyCoverage,
                    Description =
                        "Protects eligible personal belongings against covered loss or damage.",
                    IsActive = true
                },
                new()
                {
                    Id = SeedIds.TheftProtectionId,
                    Name = CoverageConstants.TheftProtection,
                    Description =
                        "Provides protection against eligible theft-related losses.",
                    IsActive = true
                },
                new()
                {
                    Id = SeedIds.FloodCoverageId,
                    Name = CoverageConstants.FloodCoverage,
                    Description =
                        "Provides protection against eligible flood-related property damage.",
                    IsActive = true
                },
                new()
                {
                    Id = SeedIds.EarthquakeCoverageId,
                    Name = CoverageConstants.EarthquakeCoverage,
                    Description =
                        "Provides protection against eligible earthquake-related property damage.",
                    IsActive = true
                },
                new()
                {
                    Id = SeedIds.WildfireCoverageId,
                    Name = CoverageConstants.WildfireCoverage,
                    Description =
                        "Provides protection against eligible wildfire-related property damage.",
                    IsActive = true
                },
                new()
                {
                    Id = SeedIds.HurricaneCoverageId,
                    Name = CoverageConstants.HurricaneCoverage,
                    Description =
                        "Provides protection against eligible hurricane-related property damage.",
                    IsActive = true
                }
            };


            await _dbContext.Coverages.AddRangeAsync(
                coverages,
                cancellationToken);
        }


        private async Task SeedProductsAsync(
            CancellationToken cancellationToken)
        {
            if (await _dbContext.Products.AnyAsync(
                    cancellationToken))
            {
                return;
            }


            Product[] products =
            {
                new()
                {
                    Id = SeedIds.PropertyProtectionProductId,
                    Name = ProductConstants.PropertyProtection,
                    Description =
                        "Core protection for the dwelling, other structures, personal property, and theft risks.",
                    ProductType =
                        ProductType.PropertyProtection,
                    IsActive = true
                },
                new()
                {
                    Id =
                        SeedIds.NaturalDisasterProtectionProductId,
                    Name =
                        ProductConstants.NaturalDisasterProtection,
                    Description =
                        "Protection focused on flood, earthquake, wildfire, and hurricane risks.",
                    ProductType =
                        ProductType.NaturalDisasterProtection,
                    IsActive = true
                },
                new()
                {
                    Id =
                        SeedIds.ComprehensiveProtectionProductId,
                    Name =
                        ProductConstants.ComprehensiveProtection,
                    Description =
                        "Comprehensive protection combining property and natural-disaster coverages.",
                    ProductType =
                        ProductType.ComprehensiveProtection,
                    IsActive = true
                }
            };


            await _dbContext.Products.AddRangeAsync(
                products,
                cancellationToken);
        }


        private async Task SeedProductCoveragesAsync(
            CancellationToken cancellationToken)
        {
            if (await _dbContext.ProductCoverages.AnyAsync(
                    cancellationToken))
            {
                return;
            }


            // Illustrative limits: dwelling 100%, detached structures 20%,
            // and personal property 50% of property value. CoverageCharge
            // is a flat annual INR add-on; Admin configuration may adjust
            // these limits and charges for future quotes.
            ProductCoverage[] mappings =
            {
                CreateProductCoverage(
                    SeedIds.PropertyDwellingMappingId,
                    SeedIds.PropertyProtectionProductId,
                    SeedIds.DwellingCoverageId,
                    CoverageType.Mandatory,
                    250m,
                    100m,
                    1),


                CreateProductCoverage(
                    SeedIds.PropertyOtherStructuresMappingId,
                    SeedIds.PropertyProtectionProductId,
                    SeedIds.OtherStructuresCoverageId,
                    CoverageType.Mandatory,
                    75m,
                    20m,
                    2),


                CreateProductCoverage(
                    SeedIds.PropertyPersonalPropertyMappingId,
                    SeedIds.PropertyProtectionProductId,
                    SeedIds.PersonalPropertyCoverageId,
                    CoverageType.Optional,
                    100m,
                    50m,
                    3),


                CreateProductCoverage(
                    SeedIds.PropertyTheftMappingId,
                    SeedIds.PropertyProtectionProductId,
                    SeedIds.TheftProtectionId,
                    CoverageType.Optional,
                    80m,
                    25m,
                    4),


                CreateProductCoverage(
                    SeedIds.DisasterFloodMappingId,
                    SeedIds.NaturalDisasterProtectionProductId,
                    SeedIds.FloodCoverageId,
                    CoverageType.Mandatory,
                    175m,
                    50m,
                    1),


                CreateProductCoverage(
                    SeedIds.DisasterEarthquakeMappingId,
                    SeedIds.NaturalDisasterProtectionProductId,
                    SeedIds.EarthquakeCoverageId,
                    CoverageType.Optional,
                    150m,
                    50m,
                    2),


                CreateProductCoverage(
                    SeedIds.DisasterWildfireMappingId,
                    SeedIds.NaturalDisasterProtectionProductId,
                    SeedIds.WildfireCoverageId,
                    CoverageType.Optional,
                    140m,
                    50m,
                    3),


                CreateProductCoverage(
                    SeedIds.DisasterHurricaneMappingId,
                    SeedIds.NaturalDisasterProtectionProductId,
                    SeedIds.HurricaneCoverageId,
                    CoverageType.Optional,
                    160m,
                    50m,
                    4),


                CreateProductCoverage(
                    SeedIds.ComprehensiveDwellingMappingId,
                    SeedIds.ComprehensiveProtectionProductId,
                    SeedIds.DwellingCoverageId,
                    CoverageType.Mandatory,
                    // Illustrative flat annual INR charge; limit is 100% of property value.
                    250m,
                    100m,
                    1),


                CreateProductCoverage(
                    SeedIds.ComprehensiveOtherStructuresMappingId,
                    SeedIds.ComprehensiveProtectionProductId,
                    SeedIds.OtherStructuresCoverageId,
                    CoverageType.Mandatory,
                    // Illustrative flat annual INR charge; configured limit is 20%.
                    75m,
                    20m,
                    2),


                CreateProductCoverage(
                    SeedIds.ComprehensivePersonalPropertyMappingId,
                    SeedIds.ComprehensiveProtectionProductId,
                    SeedIds.PersonalPropertyCoverageId,
                    CoverageType.Mandatory,
                    // Illustrative flat annual INR charge; configured limit is 50%.
                    100m,
                    50m,
                    3),


                CreateProductCoverage(
                    SeedIds.ComprehensiveTheftMappingId,
                    SeedIds.ComprehensiveProtectionProductId,
                    SeedIds.TheftProtectionId,
                    CoverageType.Optional,
                    80m,
                    25m,
                    4),


                CreateProductCoverage(
                    SeedIds.ComprehensiveFloodMappingId,
                    SeedIds.ComprehensiveProtectionProductId,
                    SeedIds.FloodCoverageId,
                    CoverageType.Optional,
                    175m,
                    50m,
                    5),


                CreateProductCoverage(
                    SeedIds.ComprehensiveEarthquakeMappingId,
                    SeedIds.ComprehensiveProtectionProductId,
                    SeedIds.EarthquakeCoverageId,
                    CoverageType.Optional,
                    150m,
                    50m,
                    6),


                CreateProductCoverage(
                    SeedIds.ComprehensiveWildfireMappingId,
                    SeedIds.ComprehensiveProtectionProductId,
                    SeedIds.WildfireCoverageId,
                    CoverageType.Optional,
                    140m,
                    50m,
                    7),


                CreateProductCoverage(
                    SeedIds.ComprehensiveHurricaneMappingId,
                    SeedIds.ComprehensiveProtectionProductId,
                    SeedIds.HurricaneCoverageId,
                    CoverageType.Optional,
                    160m,
                    50m,
                    8)
            };


            await _dbContext.ProductCoverages.AddRangeAsync(
                mappings,
                cancellationToken);
        }


        private async Task SeedPricingConfigurationAsync(
            CancellationToken cancellationToken)
        {
            bool activePricingExists =
                await _dbContext.PricingConfigurations
                    .AnyAsync(
                        configuration =>
                            configuration.IsActive,
                        cancellationToken);


            if (activePricingExists)
            {
                return;
            }


            await _dbContext.PricingConfigurations.AddAsync(
                new PricingConfiguration
                {
                    Id = SeedIds.PricingConfigurationId,


                    // Illustrative academic assumption: 0.5% of property value,
                    // not a verified market rate.
                    BaseRate = 0.005m,


                    // Relative risk loadings: 0%, 25%, and 50%.
                    LowRiskMultiplier = 1.00m,
                    MediumRiskMultiplier = 1.25m,
                    HighRiskMultiplier = 1.50m,


                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }


        private async Task SeedRiskConfigurationAsync(
            CancellationToken cancellationToken)
        {
            bool activeRiskConfigurationExists =
                await _dbContext.RiskConfigurations
                    .AnyAsync(
                        configuration =>
                            configuration.IsActive,
                        cancellationToken);


            if (activeRiskConfigurationExists)
            {
                return;
            }


            await _dbContext.RiskConfigurations.AddAsync(
                new RiskConfiguration
                {
                    Id = SeedIds.RiskConfigurationId,
                    // Illustrative project thresholds, not market-derived.
                    MediumPropertyValueThreshold = 500000m,
                    HighPropertyValueThreshold = 1000000m,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }


        private static ProductCoverage CreateProductCoverage(
            Guid id,
            Guid productId,
            Guid coverageId,
            CoverageType coverageType,
            decimal coverageCharge,
            decimal coverageLimitPercentage,
            int displayOrder)
        {
            return new ProductCoverage
            {
                Id = id,
                ProductId = productId,
                CoverageId = coverageId,
                CoverageType = coverageType,
                CoverageCharge = coverageCharge,
                CoverageLimitPercentage =
                    coverageLimitPercentage,
                DisplayOrder = displayOrder,
                IsActive = true
            };
        }


        private static class SeedIds
        {
            public static readonly Guid DwellingCoverageId =
                new("10000000-0000-0000-0000-000000000001");


            public static readonly Guid OtherStructuresCoverageId =
                new("10000000-0000-0000-0000-000000000002");


            public static readonly Guid PersonalPropertyCoverageId =
                new("10000000-0000-0000-0000-000000000003");


            public static readonly Guid TheftProtectionId =
                new("10000000-0000-0000-0000-000000000004");


            public static readonly Guid FloodCoverageId =
                new("10000000-0000-0000-0000-000000000005");


            public static readonly Guid EarthquakeCoverageId =
                new("10000000-0000-0000-0000-000000000006");


            public static readonly Guid WildfireCoverageId =
                new("10000000-0000-0000-0000-000000000007");


            public static readonly Guid HurricaneCoverageId =
                new("10000000-0000-0000-0000-000000000008");


            public static readonly Guid PropertyProtectionProductId =
                new("20000000-0000-0000-0000-000000000001");


            public static readonly Guid
                NaturalDisasterProtectionProductId =
                    new(
                        "20000000-0000-0000-0000-000000000002");


            public static readonly Guid
                ComprehensiveProtectionProductId =
                    new(
                        "20000000-0000-0000-0000-000000000003");


            public static readonly Guid PricingConfigurationId =
                new("30000000-0000-0000-0000-000000000001");


            public static readonly Guid RiskConfigurationId =
                new("40000000-0000-0000-0000-000000000001");


            public static readonly Guid PropertyDwellingMappingId =
                new("50000000-0000-0000-0000-000000000001");


            public static readonly Guid
                PropertyOtherStructuresMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000002");


            public static readonly Guid
                PropertyPersonalPropertyMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000003");


            public static readonly Guid PropertyTheftMappingId =
                new("50000000-0000-0000-0000-000000000004");


            public static readonly Guid DisasterFloodMappingId =
                new("50000000-0000-0000-0000-000000000005");


            public static readonly Guid DisasterEarthquakeMappingId =
                new("50000000-0000-0000-0000-000000000006");


            public static readonly Guid DisasterWildfireMappingId =
                new("50000000-0000-0000-0000-000000000007");


            public static readonly Guid DisasterHurricaneMappingId =
                new("50000000-0000-0000-0000-000000000008");


            public static readonly Guid
                ComprehensiveDwellingMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000009");


            public static readonly Guid
                ComprehensiveOtherStructuresMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000010");


            public static readonly Guid
                ComprehensivePersonalPropertyMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000011");


            public static readonly Guid
                ComprehensiveTheftMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000012");


            public static readonly Guid
                ComprehensiveFloodMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000013");


            public static readonly Guid
                ComprehensiveEarthquakeMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000014");


            public static readonly Guid
                ComprehensiveWildfireMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000015");


            public static readonly Guid
                ComprehensiveHurricaneMappingId =
                    new(
                        "50000000-0000-0000-0000-000000000016");
        }
    }
}
