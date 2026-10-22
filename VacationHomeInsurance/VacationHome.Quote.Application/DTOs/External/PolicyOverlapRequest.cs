using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.External
{
    public sealed record PolicyOverlapRequest(
        Guid CustomerId,
        Guid PropertyId,
        ProductType ProductType,
        Guid? ExcludedQuoteId = null);
}
