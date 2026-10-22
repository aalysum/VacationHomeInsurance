using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.DTOs.Payments
{
    public sealed record PaymentDto(
        Guid Id,
        string PaymentNumber,
        Guid QuoteId,
        decimal Amount,
        PaymentStatus Status,
        int AttemptNumber,
        DateTime ProcessedAtUtc,
        string Last4Digits);
}
