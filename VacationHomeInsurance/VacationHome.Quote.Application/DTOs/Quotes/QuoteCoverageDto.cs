using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Quotes
{
    public sealed record QuoteCoverageDto(
        Guid CoverageId,
        string CoverageName,
        string CoverageDescription,
        CoverageType CoverageType,
        decimal CoverageCharge,
        decimal CoverageLimitPercentage,
        decimal CoverageLimitAmount,
        int DisplayOrder);
}
