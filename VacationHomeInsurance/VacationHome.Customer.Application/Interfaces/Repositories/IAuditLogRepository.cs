using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Application.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken);
    }
}
