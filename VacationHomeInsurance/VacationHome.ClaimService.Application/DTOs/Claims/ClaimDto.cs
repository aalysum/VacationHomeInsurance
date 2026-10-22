using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class ClaimDto
    {
        public Guid Id { get; set; }


        public string ClaimNumber { get; set; } =
            string.Empty;


        public Guid CustomerId { get; set; }


        public Guid PolicyId { get; set; }


        public string PolicyNumber { get; set; } =
            string.Empty;


        public Guid PropertyId { get; set; }


        public ClaimType Type { get; set; }


        public ClaimStatus Status { get; set; }


        public string Title { get; set; } =
            string.Empty;


        public decimal ClaimedAmount { get; set; }


        public decimal? ApprovedAmount { get; set; }


        public DateTime IncidentDate { get; set; }


        public DateTime SubmittedAtUtc { get; set; }


        public IReadOnlyCollection<
            ClaimCoverageDto>
            Coverages
        {
            get;
            set;
        } = Array.Empty<ClaimCoverageDto>();


        public string RowVersion { get; set; } =
            string.Empty;


        public string? AssignedClaimsManagerId
        {
            get;
            set;
        }


        public string? AssignedClaimsManagerName
        {
            get;
            set;
        }


        public string? AssignedClaimsManagerEmail
        {
            get;
            set;
        }


        public DateTime? AssignedAtUtc { get; set; }
    }
}
