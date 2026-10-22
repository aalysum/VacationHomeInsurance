using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Application.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task AddAsync(
            Customers customer,
            CancellationToken cancellationToken);


        Task<Customers?> GetByIdAsync(
            Guid customerId,
            CancellationToken cancellationToken);


        Task<Customers?> GetByAuthUserIdAsync(
            string authUserId,
            CancellationToken cancellationToken);
    }
}
