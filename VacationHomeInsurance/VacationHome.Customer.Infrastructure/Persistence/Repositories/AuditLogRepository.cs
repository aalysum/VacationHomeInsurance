using VacationHome.Customer.Application
    .Interfaces.Repositories;
using VacationHome.Customer.Domain.Entities;
using VacationHome.Customer.Infrastructure
    .Persistence.Context;


namespace VacationHome.Customer.Infrastructure
    .Persistence.Repositories
{
    public class AuditLogRepository
        : IAuditLogRepository
    {
        private readonly CustomerDbContext _context;


        public AuditLogRepository(
            CustomerDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken)
        {
            await _context.AuditLogs.AddAsync(
                auditLog,
                cancellationToken);
        }
    }
}
