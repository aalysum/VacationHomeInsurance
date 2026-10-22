using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Infrastructure.Persistence;


namespace VacationHome.PolicyService.Infrastructure.Repositories
{
    public sealed class AuditLogRepository
        : IAuditLogRepository
    {
        private readonly PolicyDbContext _context;


        public AuditLogRepository(
            PolicyDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            string action,
            string entityType,
            string entityId,
            Guid actorId,
            string details,
            CancellationToken cancellationToken)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorId = actorId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                CreatedAtUtc = DateTime.UtcNow
            };


            await _context.AuditLogs.AddAsync(
                auditLog,
                cancellationToken);


            await _context.SaveChangesAsync(
                cancellationToken);
        }


        public async Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}
