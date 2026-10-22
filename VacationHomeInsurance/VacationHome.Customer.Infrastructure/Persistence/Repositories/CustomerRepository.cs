using Microsoft.EntityFrameworkCore;
using VacationHome.Customer.Application
    .Interfaces.Repositories;
using VacationHome.Customer.Domain.Entities;
using VacationHome.Customer.Infrastructure
    .Persistence.Context;


namespace VacationHome.Customer.Infrastructure
    .Persistence.Repositories
{
    public class CustomerRepository
        : ICustomerRepository
    {
        private readonly CustomerDbContext _context;


        public CustomerRepository(
            CustomerDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            Customers customer,
            CancellationToken cancellationToken)
        {
            await _context.Customers.AddAsync(
                customer,
                cancellationToken);
        }


        public async Task<Customers?> GetByIdAsync(
            Guid customerId,
            CancellationToken cancellationToken)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(
                    x => x.Id == customerId,
                    cancellationToken);
        }


        public async Task<Customers?> GetByAuthUserIdAsync(
            string authUserId,
            CancellationToken cancellationToken)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(
                    x => x.AuthUserId == authUserId,
                    cancellationToken);
        }
    }
}
