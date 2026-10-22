namespace VacationHome.ProductQuote.Application.DTOs.External
{
    public sealed record PolicyOverlapResult(
        bool HasConflict,
        string? ConflictCode,
        string? Message);
}
