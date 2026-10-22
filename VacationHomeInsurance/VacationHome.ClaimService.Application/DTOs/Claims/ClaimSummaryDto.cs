using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class ClaimSummaryDto
    {
        public Guid Id { get; set; }


        public string ClaimNumber { get; set; } =
            string.Empty;


        public Guid CustomerId { get; set; }


        public Guid PolicyId { get; set; }


        public string PolicyNumber { get; set; } =
            string.Empty;


        public ClaimType Type { get; set; }


        public ClaimStatus Status { get; set; }


        public string Title { get; set; } =
            string.Empty;


        public decimal ClaimedAmount { get; set; }


        public decimal? ApprovedAmount { get; set; }


        public int CoverageCount { get; set; }


        public DateTime SubmittedAtUtc { get; set; }


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
    }
}
