using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Domain.ValueObjects
{
    public sealed record ClaimTimelineEntry(
        ClaimStatus Status,
        string Comments,
        string ChangedBy,
        DateTime ChangedAtUtc);
}
