namespace VacationHome.Auth.Application.Interfaces.Services
{
    public interface IPasswordResetNotificationService
    {
        Task SendPasswordResetAsync(
            string email,
            string rawToken,
            CancellationToken cancellationToken);
    }
}
