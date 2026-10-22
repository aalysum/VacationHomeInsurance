namespace VacationHome.PolicyService.Application.DTOs.External
{
    public sealed record RenewalQuoteResultDto(
        Guid QuoteId,
        string QuoteNumber);
}
