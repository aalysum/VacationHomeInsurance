namespace VacationHome.ProductQuote.Application.DTOs.Internal
{
    public sealed record CreateRenewalQuoteRequest(
        Guid PolicyId,
        Guid CustomerId);
}
