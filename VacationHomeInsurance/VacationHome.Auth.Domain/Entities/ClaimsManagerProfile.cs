using VacationHome.Auth.Domain.Enums;


namespace VacationHome.Auth.Domain.Entities
{


    public class ClaimsManagerApplication
    {
        public Guid Id { get; set; }


        public string UserId { get; set; } = string.Empty;


        public int YearsOfExperience { get; set; }


        public string ProfessionalSummary { get; set; } = string.Empty;


        public string VerificationDocumentPath { get; set; } = string.Empty;


        public ClaimsManagerApprovalStatus Status { get; set; }
            = ClaimsManagerApprovalStatus.PendingApproval;


        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;


        public DateTime? ApprovedAtUtc { get; set; }


        public string? ApprovedByUserId { get; set; }


        public string? RejectionReason { get; set; }
        public byte[] RowVersion { get; set; } =
            Array.Empty<byte>();
    }
}
