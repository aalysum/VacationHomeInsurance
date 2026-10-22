using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Application.Interfaces.Services;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Application.Services
{
    public sealed class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _repository;
        private readonly ICurrentUserService _currentUser;


        public AuditService(
            IAuditLogRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }


        public async Task LogAsync(
            string entityType,
            string entityId,
            string action,
            string? details = null,
            CancellationToken cancellationToken = default)
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Details = details,
                PerformedAtUtc = DateTime.UtcNow,
                PerformedBy = _currentUser.Email
            };


            await _repository.AddAsync(log, cancellationToken);
        }
    }
}
