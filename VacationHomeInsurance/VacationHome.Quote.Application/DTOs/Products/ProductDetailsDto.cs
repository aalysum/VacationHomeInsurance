using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Products
{
    public sealed record ProductDetailsDto(
        Guid Id,
        string Name,
        string Description,
        ProductType ProductType,
        bool IsActive,
        string RowVersion,
        IReadOnlyCollection<ProductCoverageDto> Coverages);
}
