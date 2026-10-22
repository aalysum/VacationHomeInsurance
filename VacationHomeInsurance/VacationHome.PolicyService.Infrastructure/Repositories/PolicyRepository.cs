using Microsoft.EntityFrameworkCore;
using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.DTOs.Policies;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;
using VacationHome.PolicyService.Infrastructure.Persistence;


namespace VacationHome.PolicyService.Infrastructure.Repositories
{
    public sealed class PolicyRepository
        : IPolicyRepository
    {
        private readonly PolicyDbContext
            _context;


        public PolicyRepository(
            PolicyDbContext context)
        {
            _context =
                context;
        }


        public async Task AddAsync(
            Policy policy,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                policy);


            await _context.Policies.AddAsync(
                policy,
                cancellationToken);
        }


        public Task UpdateAsync(
            Policy policy,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                policy);


            cancellationToken
                .ThrowIfCancellationRequested();


            _context.Policies.Update(
                policy);


            return Task.CompletedTask;
        }


        public async Task<Policy?> GetByIdAsync(
            Guid policyId,
            bool track = false,
            CancellationToken cancellationToken = default)
        {
            if (policyId == Guid.Empty)
            {
                return null;
            }


            IQueryable<Policy> query =
                _context.Policies
                    .AsSplitQuery()
                    .Include(
                        policy =>
                            policy.Coverages)
                    .Include(
                        policy =>
                            policy.Payments);


            query = query.Include(
                policy => policy.Documents);


            if (!track)
            {
                query =
                    query.AsNoTracking();
            }


            return await query
                .SingleOrDefaultAsync(
                    policy =>
                        policy.Id ==
                        policyId,
                    cancellationToken);
        }


        public async Task<Policy?> GetByQuoteIdAsync(
            Guid quoteId,
            bool track = false,
            CancellationToken cancellationToken = default)
        {
            if (quoteId == Guid.Empty)
            {
                return null;
            }


            IQueryable<Policy> query =
                _context.Policies
                    .AsSplitQuery()
                    .Include(
                        policy =>
                            policy.Coverages)
                    .Include(
                        policy =>
                            policy.Payments);


            query = query.Include(
                policy => policy.Documents);


            if (!track)
            {
                query =
                    query.AsNoTracking();
            }


            return await query
                .SingleOrDefaultAsync(
                    policy =>
                        policy.QuoteId ==
                        quoteId,
                    cancellationToken);
        }


        public async Task<
            IReadOnlyList<Policy>>
            GetCustomerPoliciesAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            if (customerId == Guid.Empty)
            {
                return Array.Empty<Policy>();
            }


            return await _context.Policies
                .AsNoTracking()
                .Include(policy => policy.Documents)
                .Where(
                    policy =>
                        policy.CustomerId ==
                        customerId)
                .OrderByDescending(
                    policy =>
                        policy.CreatedAtUtc)
                .ToListAsync(
                    cancellationToken);
        }


        public async Task<PagedResult<Policy>>
            GetAdminPoliciesAsync(
                PolicyFilterRequest request,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            ValidatePagination(
                request.PageNumber,
                request.PageSize);


            IQueryable<Policy> query =
                _context.Policies
                    .AsNoTracking();


            query = query.Include(
                policy => policy.Documents);


            if (request.Status.HasValue)
            {
                query =
                    query.Where(
                        policy =>
                            policy.Status ==
                            request.Status.Value);
            }


            if (request.ProductType.HasValue)
            {
                query =
                    query.Where(
                        policy =>
                            policy.ProductType ==
                            request.ProductType.Value);
            }


            if (
                !string.IsNullOrWhiteSpace(
                    request.ProductName)
            )
            {
                string productName =
                    request.ProductName.Trim();


                query =
                    query.Where(
                        policy =>
                            EF.Functions.Like(
                                policy.ProductName,
                                $"%{productName}%"));
            }


            int totalCount =
                await query.CountAsync(
                    cancellationToken);


            Policy[] items =
                await query
                    .OrderByDescending(
                        policy =>
                            policy.CreatedAtUtc)
                    .Skip(
                        (request.PageNumber - 1) *
                        request.PageSize)
                    .Take(
                        request.PageSize)
                    .ToArrayAsync(
                        cancellationToken);


            return new PagedResult<Policy>(
                items,
                request.PageNumber,
                request.PageSize,
                totalCount);
        }


        public async Task<
            IReadOnlyList<Policy>>
            GetPoliciesForExpiryAsync(
                DateTime utcNow,
                CancellationToken cancellationToken)
        {
            return await _context.Policies
                .Where(
                    policy =>
                        policy.Status ==
                            PolicyStatus.Active &&
                        policy.ExpiryDateUtc <=
                            utcNow)
                .OrderBy(
                    policy =>
                        policy.ExpiryDateUtc)
                .ToListAsync(
                    cancellationToken);
        }


        public async Task<List<Policy>>
            GetActivePoliciesAsync(
                Guid customerId,
                Guid propertyId,
                Guid? excludedQuoteId,
                CancellationToken cancellationToken)
        {
            if (
                customerId == Guid.Empty ||
                propertyId == Guid.Empty
            )
            {
                return new List<Policy>();
            }


            return await _context.Policies
                .AsNoTracking()
                .Include(
                    policy =>
                        policy.Coverages)
                .Where(
                    policy =>
                        policy.CustomerId ==
                            customerId &&
                        policy.PropertyId ==
                            propertyId &&
                        policy.Status ==
                            PolicyStatus.Active &&
                        (!excludedQuoteId.HasValue ||
                         policy.QuoteId != excludedQuoteId.Value))
                .OrderByDescending(
                    policy =>
                        policy.CreatedAtUtc)
                .ToListAsync(
                    cancellationToken);
        }


        public async Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }


        private static void ValidatePagination(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber),
                    "Page number must be greater than zero.");
            }


            if (
                pageSize < 1 ||
                pageSize > 100
            )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize),
                    "Page size must be between 1 and 100.");
            }
        }
    }
}
