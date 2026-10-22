using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Domain.ValueObjects
{
    public sealed record ClaimSummary(
        Guid ClaimId,
        string ClaimNumber,
        ClaimStatus Status,
        decimal ClaimedAmount,
        decimal? ApprovedAmount,
        DateTime SubmittedAtUtc);
}
