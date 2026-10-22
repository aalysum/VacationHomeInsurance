using VacationHome.PolicyService.Application.Interfaces.Services;


namespace VacationHome.PolicyService.Application.Services
{
    public sealed class NotificationService
        : INotificationService
    {
        public Task SendAsync(
            Guid customerId,
            string title,
            string message,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
