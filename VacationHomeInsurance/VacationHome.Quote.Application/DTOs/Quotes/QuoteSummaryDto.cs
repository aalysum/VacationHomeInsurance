using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Quotes
{
    public sealed record QuoteSummaryDto(
        Guid Id,
        string QuoteNumber,
        Guid PropertyId,
        string PropertyName,
        Guid ProductId,
        string ProductName,
        QuoteStatus Status,
        decimal FinalAnnualPremium,
        DateTime GeneratedAtUtc,
        DateTime ExpiresAtUtc,
        bool CustomerAccepted,
        AdminDecision? AdminDecision,
        DateTime? PaymentDeadlineUtc);
}
