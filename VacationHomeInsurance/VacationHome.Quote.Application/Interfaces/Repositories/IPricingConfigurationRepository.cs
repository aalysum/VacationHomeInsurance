using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Interfaces.Persistence
{
    public interface IPricingConfigurationRepository
    {
        Task<PricingConfiguration?> GetActiveAsync(
            bool tracking,
            CancellationToken cancellationToken);


        void Update(PricingConfiguration configuration);


        void SetOriginalRowVersion(
    PricingConfiguration configuration,
    byte[] rowVersion);
    }
}
