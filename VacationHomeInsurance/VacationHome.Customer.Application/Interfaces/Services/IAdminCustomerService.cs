using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Application.DTOs.Admin;
using VacationHome.Customer.Application.DTOs.Properties;


namespace VacationHome.Customer.Application.Interfaces.Services
{
    public interface IAdminCustomerService
    {
        Task<PagedResult<AdminCustomerSummaryResponse>>
            GetCustomersAsync(
                AdminCustomerQueryParameters query,
                CancellationToken cancellationToken);


        Task<AdminCustomerDetailsResponse>
            GetCustomerAsync(
                Guid customerId,
                CancellationToken cancellationToken);


        Task<PagedResult<VacationPropertyResponse>>
            GetCustomerPropertiesAsync(
                Guid customerId,
                AdminCustomerPropertyQueryParameters query,
                CancellationToken cancellationToken);
    }
}
