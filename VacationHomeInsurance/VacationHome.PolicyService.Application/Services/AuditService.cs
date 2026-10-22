using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Application.Interfaces.Services;


namespace VacationHome.PolicyService.Application.Services
{
    public sealed class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ICurrentUserService _currentUserService;


        public AuditService(
            IAuditLogRepository auditLogRepository,
            ICurrentUserService currentUserService)
        {
            _auditLogRepository = auditLogRepository;
            _currentUserService = currentUserService;
        }


        public async Task LogAsync(
            string action,
            string entityType,
            string entityId,
            string details,
            CancellationToken cancellationToken)
        {
            await _auditLogRepository.AddAsync(
                action,
                entityType,
                entityId,
                _currentUserService.IsAuthenticated()
                    ? _currentUserService.GetUserId()
                    : Guid.Empty,
                details,
                cancellationToken);
        }
    }
}
