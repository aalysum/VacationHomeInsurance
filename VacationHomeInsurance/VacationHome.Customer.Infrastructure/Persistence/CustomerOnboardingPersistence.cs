using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VacationHome.Customer.Application.Interfaces.Repositories;
using VacationHome.Customer.Domain.Entities;
using VacationHome.Customer.Infrastructure.Persistence.Context;


namespace VacationHome.Customer.Infrastructure.Persistence
{
    public class CustomerOnboardingPersistence
        : ICustomerOnboardingPersistence
    {
        private readonly CustomerDbContext _context;




        public CustomerOnboardingPersistence(
            CustomerDbContext context)
        {
            _context = context;
        }


        public async Task<CustomerOnboardingSaveResult>
            TryCreateAsync(
                Customers customer,
                AuditLog auditLog,
                CancellationToken cancellationToken)
        {
            await _context.Customers.AddAsync(
                customer,
                cancellationToken);


            await _context.AuditLogs.AddAsync(
                auditLog,
                cancellationToken);


            try
            {
                await _context.SaveChangesAsync(
                    cancellationToken);


                return CustomerOnboardingSaveResult.Created;
            }
            catch (DbUpdateException exception)
                when (IsUniqueConstraintViolation(exception))
            {
                _context.ChangeTracker.Clear();


                return CustomerOnboardingSaveResult
                    .DuplicateAuthUserId;
            }
        }


        private static bool IsUniqueConstraintViolation(
            DbUpdateException exception)
        {
            return exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            };
        }
    }
}
