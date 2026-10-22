using Microsoft.EntityFrameworkCore;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.Persistence;


namespace VacationHome.ClaimService.Infrastructure.Persistence
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ClaimDbContext _context;


        public UnitOfWork(
            ClaimDbContext context)
        {
            _context = context;
        }


        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(
                    cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw new ConcurrencyConflictException(
                    "The Claim was changed by another request. Refresh the Claim and try again.",
                    exception);
            }
        }
    }
}
