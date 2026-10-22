namespace VacationHome.ClaimService.Application.Interfaces.Services
{
    public interface IAuditService
    {
        Task LogAsync(
            string entityType,
            string entityId,
            string action,
            string? details = null,
            CancellationToken cancellationToken = default);
    }
}
