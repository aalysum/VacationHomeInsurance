namespace VacationHome.ProductQuote.Application.DTOs.Products
{
    public sealed record CoverageDto(
        Guid Id,
        string Name,
        string Description,
        bool IsActive);
}
