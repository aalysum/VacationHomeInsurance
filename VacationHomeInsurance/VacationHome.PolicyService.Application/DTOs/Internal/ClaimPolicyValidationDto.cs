namespace VacationHome.PolicyService.Application.DTOs.Internal
{
    public sealed record ClaimPolicyValidationDto
    {
        public bool PolicyExists { get; init; }


        public bool PolicyActive { get; init; }


        public bool CoverageExists { get; init; }


        public decimal CoverageLimitAmount { get; init; }


        public DateTime EffectiveDateUtc { get; init; }


        public DateTime ExpiryDateUtc { get; init; }
    }
}
