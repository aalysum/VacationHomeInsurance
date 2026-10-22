namespace VacationHome.Auth.Application.DTOs
{


    public class VerifyMfaRequest
    {
        public string MfaChallengeToken { get; set; } =
        string.Empty;


        public string Code { get; set; } =
        string.Empty;
    }
}
