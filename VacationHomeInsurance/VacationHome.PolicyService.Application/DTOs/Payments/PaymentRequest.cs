namespace VacationHome.PolicyService.Application.DTOs.Payments
{
    public sealed record PaymentRequest(
        Guid QuoteId,
        string CardholderName,
        string CardNumber,
        int ExpiryMonth,
        int ExpiryYear,
        string Cvv,
        decimal Amount);
}
