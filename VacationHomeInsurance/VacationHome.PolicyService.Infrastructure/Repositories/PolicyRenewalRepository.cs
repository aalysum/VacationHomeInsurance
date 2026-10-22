using Microsoft.EntityFrameworkCore;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;
using VacationHome.PolicyService.Infrastructure.Persistence;


namespace VacationHome.PolicyService.Infrastructure.Repositories
{
    public sealed class PolicyRenewalRepository
        : IPolicyRenewalRepository
    {
        private readonly PolicyDbContext _context;


        public PolicyRenewalRepository(
            PolicyDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            PolicyRenewal renewal,
            CancellationToken cancellationToken)
        {
            await _context.PolicyRenewals.AddAsync(
                renewal,
                cancellationToken);
        }


        public async Task<PolicyRenewal?> GetActiveAsync(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            return await _context.PolicyRenewals
                .FirstOrDefaultAsync(
                    x =>
                        x.ExistingPolicyId == policyId &&
                        x.Status == RenewalStatus.Pending,
                    cancellationToken);
        }


        public async Task<PolicyRenewal?> GetByIdAsync(
            Guid renewalId,
            bool track = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<PolicyRenewal> query =
                _context.PolicyRenewals;


            if (!track)
            {
                query = query.AsNoTracking();
            }


            return await query.FirstOrDefaultAsync(
                x => x.Id == renewalId,
                cancellationToken);
        }


        public async Task<PolicyRenewal?> GetByRenewalQuoteIdAsync(
            Guid quoteId,
            bool track = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<PolicyRenewal> query =
                _context.PolicyRenewals;


            if (!track)
            {
                query = query.AsNoTracking();
            }


            return await query.FirstOrDefaultAsync(
                x => x.RenewalQuoteId == quoteId,
                cancellationToken);
        }


        public async Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}
