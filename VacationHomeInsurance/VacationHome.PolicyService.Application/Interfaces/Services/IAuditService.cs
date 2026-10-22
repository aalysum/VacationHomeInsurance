namespace VacationHome.PolicyService.Application.Interfaces.Services
{
    public interface IAuditService
    {
        Task LogAsync(
            string action,
            string entityType,
            string entityId,
            string details,
            CancellationToken cancellationToken);
    }
}
