using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Products
{
    public sealed record ProductCoverageDto(
        Guid Id,
        Guid CoverageId,
        string CoverageName,
        string CoverageDescription,
        CoverageType CoverageType,
        decimal CoverageCharge,
        decimal CoverageLimitPercentage,
        int DisplayOrder,
        bool IsActive,
        string RowVersion);
}
