using Microsoft.EntityFrameworkCore;
using VacationHome.Customer.Application
    .Interfaces.Repositories;
using VacationHome.Customer.Domain.Entities;
using VacationHome.Customer.Infrastructure
    .Persistence.Context;


namespace VacationHome.Customer.Infrastructure
    .Persistence.Repositories
{
    public class CustomerProfileRepository
        : ICustomerProfileRepository
    {
        private readonly CustomerDbContext _context;


        public CustomerProfileRepository(
            CustomerDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            CustomerProfile profile,
            CancellationToken cancellationToken)
        {
            await _context.CustomerProfiles.AddAsync(
                profile,
                cancellationToken);
        }


        public async Task<CustomerProfile?>
            GetByCustomerIdAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            return await _context.CustomerProfiles
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId,
                    cancellationToken);
        }
    }
}
