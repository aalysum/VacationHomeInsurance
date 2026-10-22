using VacationHome.ClaimService.Application.DTOs.External;


namespace VacationHome.ClaimService.Application.Interfaces.External
{
    public interface ICustomerServiceClient
    {
        Task<CustomerSummaryDto?> GetCustomerAsync(
            Guid customerId,
            CancellationToken cancellationToken = default);


        Task<CustomerStatusResponse?> GetCustomerStatusByAuthUserIdAsync(
            string authUserId,
            CancellationToken cancellationToken = default);
    }
}
