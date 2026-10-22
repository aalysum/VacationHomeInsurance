namespace VacationHome.Auth.Application.DTOs
{


    public sealed class RegisterClaimsManagerRequest
    {
        public string FirstName { get; set; } = string.Empty;


        public string LastName { get; set; } = string.Empty;


        public string Email { get; set; } = string.Empty;


        public string PhoneNumber { get; set; } = string.Empty;


        public int YearsOfExperience { get; set; }


        public string ProfessionalSummary { get; set; } = string.Empty;


        public string Password { get; set; } = string.Empty;


        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
