using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Interfaces.Persistence
{
    public interface IRiskConfigurationRepository
    {
        Task<RiskConfiguration?> GetActiveAsync(
            bool tracking,
            CancellationToken cancellationToken);
    }
}
