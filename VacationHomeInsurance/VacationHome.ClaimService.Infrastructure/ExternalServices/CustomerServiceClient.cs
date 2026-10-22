using System.Net;
using System.Net.Http.Json;
using VacationHome.ClaimService.Application.DTOs.External;
using VacationHome.ClaimService.Application.Interfaces.External;


namespace VacationHome.ClaimService.Infrastructure.ExternalServices
{
    public sealed class CustomerServiceClient
        : ICustomerServiceClient
    {
        private readonly HttpClient _httpClient;


        public CustomerServiceClient(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<CustomerSummaryDto?> GetCustomerAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    $"internal/customers/{customerId}",
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerSummaryDto>(
                cancellationToken: cancellationToken);
        }


        public async Task<CustomerStatusResponse?> GetCustomerStatusByAuthUserIdAsync(
            string authUserId,
            CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetFromJsonAsync<CustomerStatusResponse>(
                $"internal/customers/by-auth-user/{authUserId}/status",
                cancellationToken);
        }
    }
}
