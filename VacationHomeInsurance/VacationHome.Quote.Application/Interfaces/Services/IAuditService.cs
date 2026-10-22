namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface IAuditService
    {
        Task AuditAsync(
            string action,
            string entityType,
            string entityId,
            string? oldValues,
            string? newValues,
            CancellationToken cancellationToken);
    }
}
