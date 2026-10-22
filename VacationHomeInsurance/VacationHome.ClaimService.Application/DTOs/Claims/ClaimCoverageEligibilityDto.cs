namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed record ClaimCoverageEligibilityDto(
        bool RulesConfigured,
        string Message,
        IReadOnlyCollection<EligibleClaimCoverageDto> Coverages);

    public sealed record EligibleClaimCoverageDto(
        Guid CoverageId,
        string CoverageName,
        decimal CoverageLimitAmount,
        decimal RemainingLimitAmount);
}
