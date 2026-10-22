using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Application.Interfaces.Repositories
{


    public interface IAuditLogRepository
    {
        Task AddAsync(
            AuditLog entity,
            CancellationToken cancellationToken);


        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}
