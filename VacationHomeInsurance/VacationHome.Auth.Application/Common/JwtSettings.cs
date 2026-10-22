namespace VacationHome.Auth.Application.Common
{
    public sealed class JwtSettings
    {
        public string Issuer { get; set; } =
            string.Empty;


        public string Audience { get; set; } =
            string.Empty;


        public string SecretKey { get; set; } =
            string.Empty;


        public int AccessTokenExpirationMinutes
        {
            get;
            set;
        }


        public int RefreshTokenExpirationDays
        {
            get;
            set;
        }


        public int MfaChallengeExpirationMinutes
        {
            get;
            set;
        } = 5;


        public int ClaimsManagerApplicationTokenExpirationMinutes
        {
            get;
            set;
        } = 10;


        public int InternalTokenExpirationMinutes { get; set; }= 10;
    }
}
