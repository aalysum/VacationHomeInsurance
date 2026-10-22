using VacationHome.ClaimService.Application.DTOs.Admin;
using VacationHome.ClaimService.Application.DTOs.Documents;
using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class ClaimDetailDto
    {
        public Guid Id { get; set; }


        public string ClaimNumber { get; set; } =
            string.Empty;


        public Guid CustomerId { get; set; }


        public Guid PolicyId { get; set; }


        public string PolicyNumber { get; set; } =
            string.Empty;


        public Guid PropertyId { get; set; }

        public ClaimCustomerDetailsDto? Customer { get; set; }


        public ClaimPropertyDetailsDto? Property { get; set; }


        public ClaimPolicyDetailsDto? Policy { get; set; }


        public ClaimType Type { get; set; }


        public ClaimStatus Status { get; set; }


        public DateTime IncidentDate { get; set; }


        public string Title { get; set; } =
            string.Empty;


        public string Description { get; set; } =
            string.Empty;


        public decimal ClaimedAmount { get; set; }


        public decimal? ApprovedAmount { get; set; }


        public string? RejectionReason { get; set; }


        public DateTime SubmittedAtUtc { get; set; }


        public IReadOnlyCollection<
            ClaimCoverageDto>
            Coverages
        {
            get;
            set;
        } = Array.Empty<ClaimCoverageDto>();


        public IReadOnlyCollection<
            ClaimDocumentDto>
            Documents
        {
            get;
            set;
        } = Array.Empty<ClaimDocumentDto>();


        public IReadOnlyCollection<
            ClaimStatusHistoryDto>
            Timeline
        {
            get;
            set;
        } = Array.Empty<
            ClaimStatusHistoryDto>();


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


        public IReadOnlyCollection<
            ClaimAssignmentHistoryDto>
            AssignmentHistory
        {
            get;
            set;
        } = Array.Empty<
            ClaimAssignmentHistoryDto>();
    }
}
