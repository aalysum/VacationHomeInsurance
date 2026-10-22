using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.External
{
    public sealed record InternalPolicyDto(
        Guid PolicyId,
        Guid QuoteId,
        Guid CustomerId,
        Guid PropertyId,
        ProductType ProductType,
        string ProductName,
        DateTime EffectiveDateUtc,
        DateTime ExpiryDateUtc);
}
