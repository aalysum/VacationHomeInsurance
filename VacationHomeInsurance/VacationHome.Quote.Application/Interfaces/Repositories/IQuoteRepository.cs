using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Admin;
using VacationHome.ProductQuote.Application.DTOs.Quotes;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.Interfaces.Persistence
{
    public interface IQuoteRepository
    {
        Task<Quote?> GetByIdAsync(
            Guid quoteId,
            bool tracking,
            CancellationToken cancellationToken);


        Task<Quote?> GetDetailsAsync(
            Guid quoteId,
            bool tracking,
            CancellationToken cancellationToken);

        Task<IReadOnlyCollection<ProductType>>
            GetInFlightProductTypesAsync(
                Guid customerId,
                Guid propertyId,
                Guid excludedQuoteId,
                DateTime asOfUtc,
                CancellationToken cancellationToken);


        Task<Quote?> FindIdenticalActiveQuoteAsync(
            Guid customerId,
            Guid propertyId,
            Guid productId,
            decimal propertyValue,
            int constructionYear,
            string occupancyFrequency,
            bool hasSecuritySystem,
            int previousClaimsCount,
            bool hasDisasterExposure,
            Guid pricingConfigurationId,
            IReadOnlyCollection<Guid>
                normalizedOptionalCoverageIds,
            CancellationToken cancellationToken);




        Task<PagedResult<Quote>>
            SearchCustomerQuotesAsync(
                Guid customerId,
                QuoteSearchRequest request,
                CancellationToken cancellationToken);


        Task<PagedResult<Quote>>
            SearchAdminQuotesAsync(
                AdminQuoteSearchRequest request,
                CancellationToken cancellationToken);


        Task<IReadOnlyCollection<Quote>>
            GetEligibleForExpiryAsync(
                DateTime processingTimeUtc,
                CancellationToken cancellationToken);


        void SetOriginalRowVersion(
            Quote quote,
            byte[] rowVersion);


        Task AddAsync(
            Quote quote,
            CancellationToken cancellationToken);


        void Update(Quote quote);
    }
}
