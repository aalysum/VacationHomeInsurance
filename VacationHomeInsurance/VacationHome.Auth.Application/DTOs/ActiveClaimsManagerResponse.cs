namespace VacationHome.Auth.Application.DTOs
{
    public sealed class ActiveClaimsManagerResponse
    {
        /*
         * ApplicationUser.Id is a string because Auth uses
         * ASP.NET Core Identity.
         *
         * Claim Service must store this value as a string.
         */
        public string UserId { get; init; } = string.Empty;


        public string FirstName { get; init; } = string.Empty;


        public string LastName { get; init; } = string.Empty;


        public string Email { get; init; } = string.Empty;


        public int YearsOfExperience { get; init; }


        public string ProfessionalSummary { get; init; } = string.Empty;


        public string DisplayName =>
            string.Join(
                " ",
                new[]
                {
                    FirstName,
                    LastName
                }
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(value))
                .Select(
                    value =>
                        value.Trim()));
    }
}
