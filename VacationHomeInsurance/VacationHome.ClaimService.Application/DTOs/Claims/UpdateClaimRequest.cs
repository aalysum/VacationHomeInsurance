using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class UpdateClaimRequest
    {
        public ClaimType Type { get; set; }


        public DateTime IncidentDate { get; set; }


        public string Title { get; set; } = string.Empty;


        public string Description { get; set; } = string.Empty;


        public IReadOnlyCollection<ClaimCoverageRequest> Coverages { get; set; } =
            Array.Empty<ClaimCoverageRequest>();


        public string RowVersion { get; set; } = string.Empty;
    }
}
