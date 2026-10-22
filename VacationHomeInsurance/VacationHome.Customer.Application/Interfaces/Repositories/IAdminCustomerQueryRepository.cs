using VacationHome.Customer.Application.DTOs.Admin;
using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Application.Interfaces.Repositories
{
    public interface IAdminCustomerQueryRepository
    {
        Task<(
            IReadOnlyList<AdminCustomerSummaryResponse> Items,
            int TotalCount)>
            GetCustomersAsync(
                AdminCustomerQueryParameters query,
                CancellationToken cancellationToken);


        Task<Customers?> GetCustomerWithProfileAsync(
            Guid customerId,
            CancellationToken cancellationToken);


        Task<bool> CustomerExistsAsync(
            Guid customerId,
            CancellationToken cancellationToken);


        Task<int> GetPropertyCountAsync(
            Guid customerId,
            CancellationToken cancellationToken);


        Task<(
            IReadOnlyList<VacationProperty> Items,
            int TotalCount)>
            GetCustomerPropertiesAsync(
                Guid customerId,
                AdminCustomerPropertyQueryParameters query,
                CancellationToken cancellationToken);
    }
}
