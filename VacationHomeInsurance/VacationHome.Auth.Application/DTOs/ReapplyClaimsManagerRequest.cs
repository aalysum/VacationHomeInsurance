namespace VacationHome.Auth.Application.DTOs
{
    public sealed class ReapplyClaimsManagerRequest
    {
        public string FirstName { get; set; } =
            string.Empty;


        public string LastName { get; set; } =
            string.Empty;


        public string PhoneNumber { get; set; } =
            string.Empty;


        public int YearsOfExperience { get; set; }


        public string ProfessionalSummary { get; set; } =
            string.Empty;
    }
}
