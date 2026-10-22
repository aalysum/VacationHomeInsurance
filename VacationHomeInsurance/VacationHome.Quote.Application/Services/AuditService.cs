using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Application.Interfaces.Services;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Services
{
    public sealed class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICorrelationContext _correlationContext;


        public AuditService(
            IAuditLogRepository auditLogRepository,
            ICurrentUserService currentUserService,
            ICorrelationContext correlationContext)
        {
            _auditLogRepository = auditLogRepository;
            _currentUserService = currentUserService;
            _correlationContext = correlationContext;
        }


        public async Task AuditAsync(
            string action,
            string entityType,
            string entityId,
            string? oldValues,
            string? newValues,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException(
                    "Audit action is required.",
                    nameof(action));
            }


            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new ArgumentException(
                    "Audit entity type is required.",
                    nameof(entityType));
            }


            if (string.IsNullOrWhiteSpace(entityId))
            {
                throw new ArgumentException(
                    "Audit entity identifier is required.",
                    nameof(entityId));
            }


            string performedByRole =
                string.IsNullOrWhiteSpace(
                    _currentUserService.Role)
                    ? "System"
                    : _currentUserService.Role.Trim();


            string correlationId =
                string.IsNullOrWhiteSpace(
                    _correlationContext.CorrelationId)
                    ? Guid.NewGuid().ToString()
                    : _correlationContext.CorrelationId;


            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entityType.Trim(),
                EntityId = entityId.Trim(),
                Action = action.Trim(),
                OldValues = NormalizeOptional(oldValues),
                NewValues = NormalizeOptional(newValues),
                PerformedByRole = performedByRole,
                CorrelationId = correlationId,
                PerformedAtUtc = DateTime.UtcNow
            };


            await _auditLogRepository.AddAsync(
                auditLog,
                cancellationToken);
        }


        private static string? NormalizeOptional(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
