namespace VacationHome.Auth.Application.DTOs
{
    public sealed class ConfirmMfaSetupRequest
    {
        public string Code { get; set; } =
        string.Empty;
    }
}
