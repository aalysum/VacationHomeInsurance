using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Infrastructure.Persistence;


namespace VacationHome.ProductQuote.Infrastructure.Persistence.Repositories
{
    public sealed class AuditLogRepository :
        IAuditLogRepository
    {
        private readonly ProductQuoteDbContext _dbContext;


        public AuditLogRepository(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(auditLog);


            await _dbContext.AuditLogs.AddAsync(
                auditLog,
                cancellationToken);
        }
    }
}
