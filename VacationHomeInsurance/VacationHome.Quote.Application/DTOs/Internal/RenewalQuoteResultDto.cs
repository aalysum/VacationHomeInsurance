namespace VacationHome.ProductQuote.Application.DTOs.Internal
{
    public sealed record RenewalQuoteResultDto(
        Guid QuoteId,
        string QuoteNumber,
        decimal FinalAnnualPremium,
        DateTime ExpiresAtUtc);
}
