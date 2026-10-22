namespace VacationHome.PolicyService.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(
            Guid customerId,
            string title,
            string message,
            CancellationToken cancellationToken);
    }
}
