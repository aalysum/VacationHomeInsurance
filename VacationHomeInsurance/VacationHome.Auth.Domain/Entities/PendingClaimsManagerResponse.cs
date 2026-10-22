namespace VacationHome.Auth.Application.DTOs
{
    public sealed class
        PendingClaimsManagerResponse
    {
        public Guid ApplicationId { get; set; }


        public int YearsOfExperience { get; set; }


        public string ProfessionalSummary { get; set; } =
            string.Empty;


        public DateTime SubmittedAtUtc { get; set; }
    }
}
