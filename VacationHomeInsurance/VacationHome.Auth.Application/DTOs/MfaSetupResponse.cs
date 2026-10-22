namespace VacationHome.Auth.Application.DTOs
{


    public sealed class MfaSetupResponse
    {
        public string SharedKey { get; set; } =
        string.Empty;


        public string AuthenticatorUri { get; set; } =
        string.Empty;
    }
}
