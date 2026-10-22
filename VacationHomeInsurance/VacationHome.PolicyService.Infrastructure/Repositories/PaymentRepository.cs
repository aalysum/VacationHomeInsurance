using Microsoft.EntityFrameworkCore;
using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.DTOs.Payments;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;
using VacationHome.PolicyService.Infrastructure.Persistence;


namespace VacationHome.PolicyService.Infrastructure.Repositories
{
    public sealed class PaymentRepository : IPaymentRepository
    {
        private readonly PolicyDbContext _context;


        public PaymentRepository(
            PolicyDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            Payment payment,
            CancellationToken cancellationToken)
        {
            await _context.Payments.AddAsync(
                payment,
                cancellationToken);
        }


        public async Task<Payment?> GetByIdAsync(
            Guid paymentId,
            bool track = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Payment> query =
                _context.Payments;


            if (!track)
            {
                query = query.AsNoTracking();
            }


            return await query.FirstOrDefaultAsync(
                x => x.Id == paymentId,
                cancellationToken);
        }


        public async Task<Payment?> GetSuccessfulPaymentAsync(
            Guid quoteId,
            CancellationToken cancellationToken)
        {
            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.QuoteId == quoteId &&
                        x.Status == PaymentStatus.Success,
                    cancellationToken);
        }


        public async Task<int> GetAttemptCountAsync(
            Guid quoteId,
            CancellationToken cancellationToken)
        {
            return await _context.Payments
                .CountAsync(
                    x => x.QuoteId == quoteId,
                    cancellationToken);
        }


        public async Task<IReadOnlyList<Payment>>
            GetCustomerPaymentsAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.ProcessedAtUtc)
                .ToListAsync(cancellationToken);
        }


        public async Task<PagedResult<Payment>>
            GetAdminPaymentsAsync(
                PaymentFilterRequest request,
                CancellationToken cancellationToken)
        {
            IQueryable<Payment> query =
                _context.Payments.AsNoTracking();


            if (request.Status.HasValue)
            {
                query = query.Where(
                    x => x.Status == request.Status.Value);
            }


            var totalCount =
                await query.CountAsync(
                    cancellationToken);


            var items =
                await query
                    .OrderByDescending(x => x.ProcessedAtUtc)
                    .Skip(
                        (request.PageNumber - 1)
                        * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);


            return new PagedResult<Payment>(
                items,
                request.PageNumber,
                request.PageSize,
                totalCount);
        }


        public async Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}
