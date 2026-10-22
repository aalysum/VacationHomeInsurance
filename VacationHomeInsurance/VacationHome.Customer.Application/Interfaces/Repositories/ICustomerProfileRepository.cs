using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Application.Interfaces.Repositories
{
    public interface ICustomerProfileRepository
    {
        Task AddAsync(
            CustomerProfile profile,
            CancellationToken cancellationToken);


        Task<CustomerProfile?> GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken);
    }
}
