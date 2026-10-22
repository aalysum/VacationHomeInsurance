using Microsoft.EntityFrameworkCore;
using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure.Persistence.Repositories
{
    public sealed class AuditLogRepository
        : IAuditLogRepository
    {
        private readonly ClaimDbContext _context;


        public AuditLogRepository(
            ClaimDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken = default)
        {
            await _context.AuditLogs.AddAsync(
                auditLog,
                cancellationToken);
        }


        public async Task<IReadOnlyList<AuditLog>>
            GetByEntityAsync(
                string entityType,
                string entityId,
                CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(x =>
                    x.EntityType == entityType &&
                    x.EntityId == entityId)
                .OrderByDescending(x => x.PerformedAtUtc)
                .ToListAsync(cancellationToken);
        }
    }
}
