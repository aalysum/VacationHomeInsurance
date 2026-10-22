using Microsoft.EntityFrameworkCore;
using VacationHome.Customer.Application
    .Exceptions;
using VacationHome.Customer.Application
    .Interfaces.Repositories;
using VacationHome.Customer.Infrastructure
    .Persistence.Context;


namespace VacationHome.Customer.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CustomerDbContext _context;


        public UnitOfWork(
            CustomerDbContext context)
        {
            _context = context;
        }


        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                return await _context.SaveChangesAsync(
                    cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException();
            }
        }
    }
}
