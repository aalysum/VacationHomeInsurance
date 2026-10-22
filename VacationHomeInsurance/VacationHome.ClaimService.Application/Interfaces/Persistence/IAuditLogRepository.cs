using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Application.Interfaces.Persistence
{
    public interface IAuditLogRepository
    {
        Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken = default);


        Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
            string entityType,
            string entityId,
            CancellationToken cancellationToken = default);
    }
}
