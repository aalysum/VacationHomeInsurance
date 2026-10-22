namespace VacationHome.ProductQuote.Application.DTOs.External
{
    public sealed record CustomerStatusDto(
        Guid CustomerId,
        string AuthUserId,
        bool IsActive);
}
