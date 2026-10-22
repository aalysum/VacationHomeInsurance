namespace VacationHome.PolicyService.Application.Interfaces.Persistence
{
    public interface IAuditLogRepository
    {
        Task AddAsync(
            string action,
            string entityType,
            string entityId,
            Guid actorId,
            string details,
            CancellationToken cancellationToken);


        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}
