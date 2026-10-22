using VacationHome.Auth.Application.Interfaces.Repositories;
using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Infrastructure.Persistence.Context;


namespace VacationHome.Auth.Infrastructure.Persistence.Repositories
{


    public class AuditLogRepository
        : IAuditLogRepository
    {
        private readonly AuthDbContext _dbContext;


        public AuditLogRepository(
            AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task AddAsync(
            AuditLog entity,
            CancellationToken cancellationToken)
        {
            await _dbContext.AuditLogs
                .AddAsync(entity, cancellationToken);
        }


        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
