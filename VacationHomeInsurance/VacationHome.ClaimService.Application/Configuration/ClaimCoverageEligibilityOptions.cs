using VacationHome.ClaimService.Domain.Enums;

namespace VacationHome.ClaimService.Application.Configuration
{
    public sealed class ClaimCoverageEligibilityOptions
    {
        public const string SectionName =
            "ClaimCoverageEligibility";

        public List<ClaimCoverageEligibilityRule> Rules { get; set; } =
            [];
    }

    public sealed class ClaimCoverageEligibilityRule
    {
        public ProductType ProductType { get; set; }

        public ClaimType IncidentType { get; set; }

        public List<Guid> CoverageIds { get; set; } =
            [];

        public bool UsePolicyCoverages { get; set; }
    }
}
