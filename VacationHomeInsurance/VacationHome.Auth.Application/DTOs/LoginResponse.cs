namespace VacationHome.Auth.Application.DTOs
{
    public sealed class LoginResponse
    {
        public string? AccessToken { get; set; }


        public string? RefreshToken { get; set; }


        public DateTime? ExpiresAtUtc { get; set; }


        public bool RequiresMfa { get; set; }


        public string? MfaChallengeToken { get; set; }


        public bool RequiresClaimsManagerAction
        {
            get;
            set;
        }


        public string? ClaimsManagerApplicationToken
        {
            get;
            set;
        }


        public string? ClaimsManagerApplicationStatus
        {
            get;
            set;
        }


        public string? RejectionReason { get; set; }
    }
}
