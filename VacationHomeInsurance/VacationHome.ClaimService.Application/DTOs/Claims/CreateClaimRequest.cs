using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class CreateClaimRequest
    {
        public Guid PolicyId { get; set; }


        public Guid PropertyId { get; set; }


        public ClaimType Type { get; set; }


        public DateTime IncidentDate { get; set; }


        public string Title { get; set; } = string.Empty;


        public string Description { get; set; } = string.Empty;


        public IReadOnlyCollection<ClaimCoverageRequest> Coverages { get; set; } =
            Array.Empty<ClaimCoverageRequest>();
    }
}
