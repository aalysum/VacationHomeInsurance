using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Repositories
{
    public sealed class QuoteApprovalHistoryRepository :
        IQuoteApprovalHistoryRepository
    {
        private readonly ProductQuoteDbContext _dbContext;


        public QuoteApprovalHistoryRepository(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task AddAsync(
            QuoteApprovalHistory history,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(history);


            await _dbContext
                .Set<QuoteApprovalHistory>()
                .AddAsync(
                    history,
                    cancellationToken);
        }
    }
}
