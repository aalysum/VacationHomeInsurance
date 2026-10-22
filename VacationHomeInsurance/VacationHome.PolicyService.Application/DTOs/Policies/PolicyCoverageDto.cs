namespace VacationHome.PolicyService.Application.DTOs.Policies
{
    public sealed record PolicyCoverageDto(
        Guid CoverageId,
        string CoverageName,
        string CoverageDescription,
        bool IsMandatory,
        decimal CoverageCharge,
        decimal CoverageLimitPercentage,
        decimal CoverageLimitAmount);
}
