namespace VacationHome.Auth.Application.DTOs
{
    public sealed class ClaimsManagerApplicationStatusResponse
    {
        public Guid ApplicationId { get; set; }


        public string Status { get; set; } =
            string.Empty;


        public string? RejectionReason { get; set; }


        public DateTime SubmittedAtUtc { get; set; }


        public DateTime? ApprovedAtUtc { get; set; }
    }
}
