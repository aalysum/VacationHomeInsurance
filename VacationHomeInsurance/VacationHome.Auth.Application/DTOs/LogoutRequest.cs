namespace VacationHome.Auth.Application.DTOs
{
    public sealed class LogoutRequest
    {
        public string RefreshToken { get; set; } =
        string.Empty;
    }
}
