namespace VacationHome.ClaimService.Application.DTOs.Admin
{
    public sealed class ClaimAssignmentHistoryDto
    {
        public Guid Id { get; set; }


        public string? PreviousClaimsManagerId
        {
            get;
            set;
        }


        public string? PreviousClaimsManagerName
        {
            get;
            set;
        }


        public string? PreviousClaimsManagerEmail
        {
            get;
            set;
        }


        public string ClaimsManagerId { get; set; } =
            string.Empty;


        public string ClaimsManagerName { get; set; } =
            string.Empty;


        public string ClaimsManagerEmail { get; set; } =
            string.Empty;


        public string? Reason { get; set; }


        public string AssignedBy { get; set; } =
            string.Empty;


        public DateTime AssignedAtUtc { get; set; }
    }
}
