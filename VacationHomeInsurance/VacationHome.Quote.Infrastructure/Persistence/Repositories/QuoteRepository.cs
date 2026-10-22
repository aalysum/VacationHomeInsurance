using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Admin;
using VacationHome.ProductQuote.Application.DTOs.Quotes;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Repositories
{
    public sealed class QuoteRepository :
        IQuoteRepository
    {
        private readonly ProductQuoteDbContext _dbContext;


        public QuoteRepository(
            ProductQuoteDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<Quote?> GetByIdAsync(
            Guid quoteId,
            bool tracking,
            CancellationToken cancellationToken)
        {
            IQueryable<Quote> query =
                _dbContext.Set<Quote>();


            if (!tracking)
            {
                query = query.AsNoTracking();
            }


            return await query.SingleOrDefaultAsync(
                quote => quote.Id == quoteId,
                cancellationToken);
        }


        public async Task<Quote?> GetDetailsAsync(
            Guid quoteId,
            bool tracking,
            CancellationToken cancellationToken)
        {
            IQueryable<Quote> query =
                _dbContext
                    .Set<Quote>()
                    .AsSplitQuery()
                    .Include(quote =>
                        quote.Coverages)
                    .Include(quote =>
                        quote.ApprovalHistory);


            if (!tracking)
            {
                query = query.AsNoTracking();
            }


            return await query.SingleOrDefaultAsync(
                quote => quote.Id == quoteId,
                cancellationToken);
        }

        public async Task<IReadOnlyCollection<ProductType>>
            GetInFlightProductTypesAsync(
                Guid customerId,
                Guid propertyId,
                Guid excludedQuoteId,
                DateTime asOfUtc,
                CancellationToken cancellationToken)
        {
            return await _dbContext
                .Set<Quote>()
                .AsNoTracking()
                .Where(quote =>
                    quote.CustomerId == customerId &&
                    quote.PropertyId == propertyId &&
                    quote.Id != excludedQuoteId &&
                    !quote.ConvertedPolicyId.HasValue &&
                    quote.ExpiresAtUtc > asOfUtc &&
                    (!quote.PaymentDeadlineUtc.HasValue ||
                     quote.PaymentDeadlineUtc.Value > asOfUtc) &&
                    (quote.Status ==
                         QuoteStatus.PendingAdminApproval ||
                     quote.Status == QuoteStatus.AdminApproved))
                .Select(quote => quote.ProductType)
                .Distinct()
                .ToArrayAsync(cancellationToken);
        }


        public async Task<Quote?>
            FindIdenticalActiveQuoteAsync(
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
                CancellationToken cancellationToken)
        {
            string normalizedOccupancyFrequency =
                occupancyFrequency.Trim();


            decimal normalizedPropertyValue =
                Math.Round(
                    propertyValue,
                    2,
                    MidpointRounding.AwayFromZero);


            Guid[] normalizedOptionalIds =
                normalizedOptionalCoverageIds
                    .Distinct()
                    .OrderBy(id => id)
                    .ToArray();


            /*
             * First narrow the result set using only scalar,
             * indexed Quote fields.
             */
            List<Quote> candidates =
                await _dbContext
                    .Set<Quote>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(quote =>
                        quote.Coverages)
                    .Where(quote =>
                        quote.CustomerId == customerId &&
                        quote.PropertyId == propertyId &&
                        quote.ProductId == productId &&
                        (quote.Status == QuoteStatus.Active ||
                         quote.Status == QuoteStatus.PendingAdminApproval ||
                         quote.Status == QuoteStatus.AdminApproved) &&
                        quote.PropertyValue ==
                            normalizedPropertyValue &&
                        quote.ConstructionYear ==
                            constructionYear &&
                        quote.OccupancyFrequency ==
                            normalizedOccupancyFrequency &&
                        quote.HasSecuritySystem ==
                            hasSecuritySystem &&
                        quote.PreviousClaimsCount ==
                            previousClaimsCount &&
                        quote.HasDisasterExposure ==
                            hasDisasterExposure &&
                        quote.PricingConfigurationId ==
                            pricingConfigurationId &&
                            quote.ExpiresAtUtc > DateTime.UtcNow)
                    .OrderByDescending(quote =>
                        quote.GeneratedAtUtc)
                    .ToListAsync(cancellationToken);


            /*
             * Compare normalized optional Coverage IDs after
             * the database has narrowed the candidate set.
             *
             * This is a direct comparison and does not use a
             * fingerprint or hash.
             */
            foreach (Quote candidate in candidates)
            {
                Guid[] candidateOptionalIds =
                    candidate.Coverages
                        .Where(coverage =>
                            coverage.CoverageType ==
                            CoverageType.Optional)
                        .Select(coverage =>
                            coverage.CoverageId)
                        .Distinct()
                        .OrderBy(id => id)
                        .ToArray();


                if (candidateOptionalIds.SequenceEqual(
                        normalizedOptionalIds))
                {
                    return candidate;
                }
            }


            return null;
        }






        public async Task<PagedResult<Quote>>
            SearchCustomerQuotesAsync(
                Guid customerId,
                QuoteSearchRequest request,
                CancellationToken cancellationToken)
        {
            IQueryable<Quote> query =
                _dbContext
                    .Set<Quote>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(quote =>
                        quote.Coverages)
                    .Where(quote =>
                        quote.CustomerId == customerId);


            query = ApplyCustomerFilters(
                query,
                request);


            int totalCount =
                await query.CountAsync(
                    cancellationToken);


            query = ApplyCustomerSorting(
                query,
                request);


            Quote[] items =
                await query
                    .Skip(
                        (request.PageNumber - 1) *
                        request.PageSize)
                    .Take(request.PageSize)
                    .ToArrayAsync(cancellationToken);


            return new PagedResult<Quote>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }


        public async Task<PagedResult<Quote>>
            SearchAdminQuotesAsync(
                AdminQuoteSearchRequest request,
                CancellationToken cancellationToken)
        {
            IQueryable<Quote> query =
                _dbContext
                    .Set<Quote>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(quote =>
                        quote.Coverages)
                    .Include(quote =>
                        quote.ApprovalHistory);


            query = ApplyAdminFilters(
                query,
                request);


            int totalCount =
                await query.CountAsync(
                    cancellationToken);


            query = ApplyAdminSorting(
                query,
                request);


            Quote[] items =
                await query
                    .Skip(
                        (request.PageNumber - 1) *
                        request.PageSize)
                    .Take(request.PageSize)
                    .ToArrayAsync(cancellationToken);


            return new PagedResult<Quote>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }


        public async Task AddAsync(
            Quote quote,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(quote);


            await _dbContext
                .Set<Quote>()
                .AddAsync(
                    quote,
                    cancellationToken);
        }


        public void Update(Quote quote)
        {
            ArgumentNullException.ThrowIfNull(quote);


            _dbContext
                .Set<Quote>()
                .Update(quote);
        }


        private static IQueryable<Quote>
            ApplyCustomerFilters(
                IQueryable<Quote> query,
                QuoteSearchRequest request)
        {
            if (request.PropertyId.HasValue)
            {
                query = query.Where(quote =>
                    quote.PropertyId ==
                    request.PropertyId.Value);
            }


            if (request.ProductId.HasValue)
            {
                query = query.Where(quote =>
                    quote.ProductId ==
                    request.ProductId.Value);
            }


            if (request.Status.HasValue)
            {
                query = query.Where(quote =>
                    quote.Status ==
                    request.Status.Value);
            }


            if (request.GeneratedFromUtc.HasValue)
            {
                query = query.Where(quote =>
                    quote.GeneratedAtUtc >=
                    request.GeneratedFromUtc.Value);
            }


            if (request.GeneratedToUtc.HasValue)
            {
                query = query.Where(quote =>
                    quote.GeneratedAtUtc <=
                    request.GeneratedToUtc.Value);
            }


            if (request.CustomerAccepted.HasValue)
            {
                query = request.CustomerAccepted.Value
                    ? query.Where(quote =>
                        quote.CustomerAcceptedAtUtc
                            .HasValue)
                    : query.Where(quote =>
                        !quote.CustomerAcceptedAtUtc
                            .HasValue);
            }


            if (request.AdminDecision.HasValue)
            {
                query = query.Where(quote =>
                    quote.AdminDecision ==
                    request.AdminDecision.Value);
            }
            if (request.PaymentEligible.HasValue)
            {
                DateTime utcNow = DateTime.UtcNow;


                if (request.PaymentEligible.Value)
                {
                    query = query.Where(quote =>
                        quote.Status ==
                            QuoteStatus.AdminApproved &&
                        quote.CustomerAcceptedAtUtc.HasValue &&
                        quote.AcceptedByCustomerId ==
                            quote.CustomerId &&
                        quote.AdminDecisionAtUtc.HasValue &&
                        quote.CustomerAcceptedAtUtc >
                            quote.AdminDecisionAtUtc &&
                        quote.AdminDecision ==
                            AdminDecision.Approved &&
                        quote.AdminDecisionAtUtc.HasValue &&
                        quote.ExpiresAtUtc > utcNow &&
                        (!quote.PaymentDeadlineUtc.HasValue ||
                         quote.PaymentDeadlineUtc.Value >
                            utcNow) &&
                        !quote.ConvertedPolicyId.HasValue);
                }
                else
                {
                    query = query.Where(quote =>
                        quote.Status !=
                            QuoteStatus.AdminApproved ||
                        !quote.CustomerAcceptedAtUtc.HasValue ||
                        quote.AcceptedByCustomerId !=
                            quote.CustomerId ||
                        !quote.AdminDecisionAtUtc.HasValue ||
                        quote.CustomerAcceptedAtUtc <=
                            quote.AdminDecisionAtUtc ||
                        quote.AdminDecision !=
                            AdminDecision.Approved ||
                        !quote.AdminDecisionAtUtc.HasValue ||
                        quote.ExpiresAtUtc <= utcNow ||
                        (quote.PaymentDeadlineUtc.HasValue &&
                         quote.PaymentDeadlineUtc.Value <=
                            utcNow) ||
                        quote.ConvertedPolicyId.HasValue);
                }
            }


            return query;
        }


        private static IQueryable<Quote>
            ApplyAdminFilters(
                IQueryable<Quote> query,
                AdminQuoteSearchRequest request)
        {
            if (request.CustomerId.HasValue)
            {
                query = query.Where(quote =>
                    quote.CustomerId ==
                    request.CustomerId.Value);
            }


            if (request.PropertyId.HasValue)
            {
                query = query.Where(quote =>
                    quote.PropertyId ==
                    request.PropertyId.Value);
            }


            if (request.ProductId.HasValue)
            {
                query = query.Where(quote =>
                    quote.ProductId ==
                    request.ProductId.Value);
            }


            if (request.Status.HasValue)
            {
                query = query.Where(quote =>
                    quote.Status ==
                    request.Status.Value);
            }


            if (request.GeneratedFromUtc.HasValue)
            {
                query = query.Where(quote =>
                    quote.GeneratedAtUtc >=
                    request.GeneratedFromUtc.Value);
            }


            if (request.GeneratedToUtc.HasValue)
            {
                query = query.Where(quote =>
                    quote.GeneratedAtUtc <=
                    request.GeneratedToUtc.Value);
            }


            if (request.DecisionFromUtc.HasValue)
            {
                query = query.Where(quote =>
                    quote.AdminDecisionAtUtc.HasValue &&
                    quote.AdminDecisionAtUtc.Value >=
                    request.DecisionFromUtc.Value);
            }


            if (request.DecisionToUtc.HasValue)
            {
                query = query.Where(quote =>
                    quote.AdminDecisionAtUtc.HasValue &&
                    quote.AdminDecisionAtUtc.Value <=
                    request.DecisionToUtc.Value);
            }


            return query;
        }


        private static IQueryable<Quote>
            ApplyCustomerSorting(
                IQueryable<Quote> query,
                QuoteSearchRequest request)
        {
            string sortBy =
                request.SortBy?
                    .Trim()
                    .ToLowerInvariant()
                ?? "generatedatutc";


            return (sortBy, request.SortDescending)
                switch
            {
                ("expiresatutc", false) =>
                    query.OrderBy(quote =>
                        quote.ExpiresAtUtc),


                ("expiresatutc", true) =>
                    query.OrderByDescending(quote =>
                        quote.ExpiresAtUtc),


                ("status", false) =>
                    query.OrderBy(quote =>
                        quote.Status),


                ("status", true) =>
                    query.OrderByDescending(quote =>
                        quote.Status),


                ("finalannualpremium", false) =>
                    query.OrderBy(quote =>
                        quote.FinalAnnualPremium),


                ("finalannualpremium", true) =>
                    query.OrderByDescending(quote =>
                        quote.FinalAnnualPremium),


                ("productname", false) =>
                    query.OrderBy(quote =>
                        quote.ProductName),


                ("productname", true) =>
                    query.OrderByDescending(quote =>
                        quote.ProductName),


                ("propertyname", false) =>
                    query.OrderBy(quote =>
                        quote.PropertyName),


                ("propertyname", true) =>
                    query.OrderByDescending(quote =>
                        quote.PropertyName),


                ("generatedatutc", false) =>
                    query.OrderBy(quote =>
                        quote.GeneratedAtUtc),


                _ =>
                    query.OrderByDescending(quote =>
                        quote.GeneratedAtUtc)
            };
        }


        private static IQueryable<Quote>
            ApplyAdminSorting(
                IQueryable<Quote> query,
                AdminQuoteSearchRequest request)
        {
            string sortBy =
                request.SortBy?
                    .Trim()
                    .ToLowerInvariant()
                ?? "generatedatutc";


            return (sortBy, request.SortDescending)
                switch
            {
                ("expiresatutc", false) =>
                    query.OrderBy(quote =>
                        quote.ExpiresAtUtc),


                ("expiresatutc", true) =>
                    query.OrderByDescending(quote =>
                        quote.ExpiresAtUtc),


                ("admindecisionatutc", false) =>
                    query.OrderBy(quote =>
                        quote.AdminDecisionAtUtc),


                ("admindecisionatutc", true) =>
                    query.OrderByDescending(quote =>
                        quote.AdminDecisionAtUtc),


                ("status", false) =>
                    query.OrderBy(quote =>
                        quote.Status),


                ("status", true) =>
                    query.OrderByDescending(quote =>
                        quote.Status),


                ("finalannualpremium", false) =>
                    query.OrderBy(quote =>
                        quote.FinalAnnualPremium),


                ("finalannualpremium", true) =>
                    query.OrderByDescending(quote =>
                        quote.FinalAnnualPremium),


                ("customerid", false) =>
                    query.OrderBy(quote =>
                        quote.CustomerId),


                ("customerid", true) =>
                    query.OrderByDescending(quote =>
                        quote.CustomerId),


                ("productname", false) =>
                    query.OrderBy(quote =>
                        quote.ProductName),


                ("productname", true) =>
                    query.OrderByDescending(quote =>
                        quote.ProductName),


                ("generatedatutc", false) =>
                    query.OrderBy(quote =>
                        quote.GeneratedAtUtc),


                _ =>
                    query.OrderByDescending(quote =>
                        quote.GeneratedAtUtc)
            };
        }
        public async Task<IReadOnlyCollection<Quote>>
    GetEligibleForExpiryAsync(
        DateTime processingTimeUtc,
        CancellationToken cancellationToken)
        {
            return await _dbContext.Quotes
                .Where(quote =>
                    quote.Status != QuoteStatus.Expired &&
                    quote.Status != QuoteStatus.Invalidated &&
                    quote.Status != QuoteStatus.Withdrawn &&
                    quote.Status != QuoteStatus.AdminRejected &&
                    quote.Status !=
                        QuoteStatus.ConvertedToPolicy &&
                    (
                        quote.ExpiresAtUtc <=
                            processingTimeUtc ||
                        (
                            quote.Status ==
                                QuoteStatus.AdminApproved &&
                            quote.PaymentDeadlineUtc.HasValue &&
                            quote.PaymentDeadlineUtc.Value <=
                                processingTimeUtc
                        )
                    ))
                .OrderBy(quote =>
                    quote.ExpiresAtUtc)
                .ToArrayAsync(cancellationToken);
        }


        public void SetOriginalRowVersion(
            Quote quote,
            byte[] rowVersion)
        {
            ArgumentNullException.ThrowIfNull(quote);
            ArgumentNullException.ThrowIfNull(rowVersion);


            _dbContext.Entry(quote)
                .Property(x => x.RowVersion)
                .OriginalValue = rowVersion;
        }
    }
}
