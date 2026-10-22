using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Application
    .Interfaces.Persistence;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Repositories
{
    public sealed class RiskConfigurationRepository :
        IRiskConfigurationRepository
    {
        private readonly ProductQuoteDbContext _dbContext;


        public RiskConfigurationRepository(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<RiskConfiguration?> GetActiveAsync(
            bool tracking,
            CancellationToken cancellationToken)
        {
            IQueryable<RiskConfiguration> query =
                _dbContext.RiskConfigurations;


            if (!tracking)
            {
                query = query.AsNoTracking();
            }


            return await query.SingleOrDefaultAsync(
                configuration => configuration.IsActive,
                cancellationToken);
        }
    }
}
