using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Infrastructure.Persistence;


namespace VacationHome.ProductQuote.Infrastructure.Persistence.Repositories
{
    public sealed class PricingConfigurationRepository
        : IPricingConfigurationRepository
    {
        private readonly ProductQuoteDbContext _dbContext;


        public PricingConfigurationRepository(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<PricingConfiguration?> GetActiveAsync(
            bool tracking,
            CancellationToken cancellationToken)
        {
            IQueryable<PricingConfiguration> query =
                _dbContext.PricingConfigurations;


            if (!tracking)
            {
                query = query.AsNoTracking();
            }


            return await query.SingleOrDefaultAsync(
                configuration => configuration.IsActive,
                cancellationToken);
        }


        public void SetOriginalRowVersion(
            PricingConfiguration configuration,
            byte[] rowVersion)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(rowVersion);


            _dbContext.Entry(configuration)
                .Property(x => x.RowVersion)
                .OriginalValue = rowVersion;
        }


        public void Update(
            PricingConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);


            _dbContext.PricingConfigurations.Update(
                configuration);
        }
    }
}
