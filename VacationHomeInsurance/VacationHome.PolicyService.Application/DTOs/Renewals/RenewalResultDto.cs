namespace VacationHome.PolicyService.Application.DTOs.Renewals
{
    public sealed record RenewalResultDto(
        Guid RenewalId,
        Guid QuoteId,
        string Message);
}
