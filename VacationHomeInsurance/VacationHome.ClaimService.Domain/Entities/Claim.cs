using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Domain.Entities
{
    public class Claim
    {
        public Guid Id { get; set; }


        public string ClaimNumber { get; set; } = string.Empty;


        public Guid CustomerId { get; set; }


        public Guid PolicyId { get; set; }


        public string PolicyNumber { get; set; } = string.Empty;


        public Guid PropertyId { get; set; }


        public ClaimType Type { get; set; }


        public ClaimStatus Status { get; set; }


        public DateTime IncidentDate { get; set; }


        public DateTime SubmittedAtUtc { get; set; }


        public string Title { get; set; } = string.Empty;


        public string Description { get; set; } = string.Empty;


        public decimal ClaimedAmount { get; set; }


        public decimal? ApprovedAmount { get; set; }


        public DateTime? ApprovedAtUtc { get; set; }


        public DateTime? RejectedAtUtc { get; set; }


        public DateTime? PaidAtUtc { get; set; }


        public DateTime? ClosedAtUtc { get; set; }


        public string? RejectionReason { get; set; }


        public string? InternalNotes { get; set; }


        /*
         * ASP.NET Core Identity user ID from Auth Service.
         * This is a string, not a Guid.
         */
        public string? AssignedClaimsManagerId
        {
            get;
            set;
        }


        /*
         * Display snapshots retained in Claim Service so Claim
         * details do not require an Auth Service call every time.
         */
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


        /*
         * Auth user ID or email of the Admin who performed
         * the most recent assignment.
         */
        public string? AssignedBy { get; set; }


        public string CreatedBy { get; set; } = string.Empty;


        public DateTime CreatedAtUtc { get; set; }


        public string? LastModifiedBy { get; set; }


        public DateTime? LastModifiedAtUtc { get; set; }


        public ICollection<ClaimCoverage> Coverages
        {
            get;
            set;
        } = new List<ClaimCoverage>();


        public ICollection<ClaimDocument> Documents
        {
            get;
            set;
        } = new List<ClaimDocument>();


        public ICollection<ClaimStatusHistory> StatusHistory
        {
            get;
            set;
        } = new List<ClaimStatusHistory>();


        public ICollection<ClaimAssignmentHistory> AssignmentHistory
        {
            get;
            set;
        } = new List<ClaimAssignmentHistory>();


        /*
         * SQL Server rowversion
         */
        public byte[] RowVersion { get; set; } =
            Array.Empty<byte>();
    }
}
