using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using VacationHome.PolicyService.Application.DTOs.External;
using VacationHome.PolicyService.Application
    .DTOs.Internal;
using VacationHome.PolicyService.Application
    .Interfaces.External;


namespace VacationHome.PolicyService.Infrastructure
    .External
{
    public sealed class CustomerServiceClient
        : ICustomerServiceClient
    {
        private readonly HttpClient
            _httpClient;


        private readonly IHttpContextAccessor
            _httpContextAccessor;


        public CustomerServiceClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient =
                httpClient;


            _httpContextAccessor =
                httpContextAccessor;
        }


        public async Task<CustomerSnapshotDto?> GetCustomerAsync(
            Guid customerId,
            CancellationToken cancellationToken)
        {
            if (customerId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Customer identifier is required.",
                    nameof(customerId));
            }


            using HttpRequestMessage request =
                CreateGetRequest(
                    $"internal/customers/{customerId}");


            using HttpResponseMessage response =
                await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);


            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }


            response.EnsureSuccessStatusCode();


            return await response.Content
                .ReadFromJsonAsync<CustomerSnapshotDto>(
                    cancellationToken);
        }


        public async Task<CustomerStatusDto?>
            GetCustomerStatusAsync(
                Guid authUserId,
                CancellationToken cancellationToken)
        {
            if (authUserId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Authentication user identifier is required.",
                    nameof(authUserId));
            }


            using HttpRequestMessage request =
                CreateGetRequest(
                    $"internal/customers/by-auth-user/{authUserId}/status");


            using HttpResponseMessage response =
                await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption
                        .ResponseHeadersRead,
                    cancellationToken);


            if (
                response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return null;
            }


            response.EnsureSuccessStatusCode();


            return await response.Content
                .ReadFromJsonAsync<CustomerStatusDto>(
                    cancellationToken);
        }


        public async Task<InternalPropertySnapshotDto?>
            GetPropertyAsync(
                Guid propertyId,
                CancellationToken cancellationToken)
        {
            if (propertyId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Property identifier is required.",
                    nameof(propertyId));
            }


            using HttpRequestMessage request =
                CreateGetRequest(
                    $"internal/properties/{propertyId}");


            using HttpResponseMessage response =
                await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption
                        .ResponseHeadersRead,
                    cancellationToken);


            if (
                response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return null;
            }


            response.EnsureSuccessStatusCode();


            return await response.Content
                .ReadFromJsonAsync<
                    InternalPropertySnapshotDto>(
                    cancellationToken);
        }


        private HttpRequestMessage
            CreateGetRequest(
                string relativePath)
        {
            var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    relativePath);


            string? correlationId =
                _httpContextAccessor
                    .HttpContext?
                    .TraceIdentifier;


            if (
                !string.IsNullOrWhiteSpace(
                    correlationId))
            {
                request.Headers
                    .TryAddWithoutValidation(
                        "X-Correlation-ID",
                        correlationId);
            }


            return request;
        }
    }
}
