using VacationHome.ProductQuote.Application.DTOs.Products;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Mappers
{
    public static class ProductMapper
    {
        public static ProductDetailsDto ToDetailsDto(
            this Product product)
        {
            return new ProductDetailsDto(
                product.Id,
                product.Name,
                product.Description,
                product.ProductType,
                product.IsActive,
                Convert.ToBase64String(product.RowVersion),
                product.Coverages
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x =>
                        new ProductCoverageDto(
                            x.Id,
                            x.CoverageId,
                            x.Coverage.Name,
                            x.Coverage.Description,
                            x.CoverageType,
                            x.CoverageCharge,
                            x.CoverageLimitPercentage,
                            x.DisplayOrder,
                            x.IsActive,
                            Convert.ToBase64String(
                                x.RowVersion)))
                    .ToArray());
        }
    }
}
