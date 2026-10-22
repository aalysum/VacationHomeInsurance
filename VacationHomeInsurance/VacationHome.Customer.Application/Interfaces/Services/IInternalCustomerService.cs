using System.Threading.Tasks;
using VacationHome.Customer.Application.DTOs.Internal;


namespace VacationHome.Customer.Application.Interfaces.Services
{
    public interface IInternalCustomerService
    {
        Task<CustomerStatusResponse>
            GetCustomerStatusAsync(
                Guid customerId,
                CancellationToken cancellationToken);


        Task<CustomerSummaryDto> GetCustomerAsync(
            Guid customerId,
            CancellationToken cancellationToken);
        Task<CustomerStatusResponse>
    GetCustomerStatusByAuthUserIdAsync(
        string authUserId,
        CancellationToken cancellationToken);
    }
}
