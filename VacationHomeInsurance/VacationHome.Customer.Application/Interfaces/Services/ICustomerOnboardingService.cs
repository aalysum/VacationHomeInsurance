using VacationHome.Customer.Application.DTOs.Customers;


namespace VacationHome.Customer.Application.Interfaces.Services
{
    public interface ICustomerOnboardingService
    {
        Task<CreateCustomerResponse> CreateCustomerAsync(
            CreateCustomerRequest request,
            CancellationToken cancellationToken);


        Task UpdateCustomerStatusAsync(
            string authUserId,
            UpdateCustomerStatusRequest request,
            CancellationToken cancellationToken);
    }
}
