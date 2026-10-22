using System.Net.Http.Headers;
using System.Net.Http.Json;
using VacationHome.Auth.Application.DTOs.Internal;
using VacationHome.Auth.Application.Exceptions;
using VacationHome.Auth.Application.Interfaces.Clients;
using VacationHome.Auth.Application.Interfaces.Services;


namespace VacationHome.Auth.Infrastructure.Clients
{
    public sealed class CustomerServiceClient
        : ICustomerServiceClient
    {
        private const string CorrelationIdHeader =
            "X-Correlation-ID";


        private readonly HttpClient _httpClient;


        private readonly IInternalTokenService
            _internalTokenService;


        public CustomerServiceClient(
            
            HttpClient httpClient,
            IInternalTokenService internalTokenService)
        {
            _httpClient =
                httpClient;


            _internalTokenService =
                internalTokenService;
        }


        public async Task CreateCustomerAsync(
            string authUserId,
            string firstName,
            string lastName,
            string phoneNumber,
            string correlationId,
            CancellationToken cancellationToken)
        {
            var requestBody =
                new CreateCustomerInternalRequest
                {
                    AuthUserId =
                        authUserId.Trim(),


                    FirstName =
                        firstName.Trim(),


                    LastName =
                        lastName.Trim(),


                    PhoneNumber =
                        phoneNumber.Trim()
                };


            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "customers",
                    correlationId);


            request.Content =
                JsonContent.Create(
                    requestBody);


            await SendAndEnsureSuccessAsync(
                request,
                "customer onboarding",
                cancellationToken);
        }


        public async Task SynchronizeCustomerStatusAsync(
            string authUserId,
            string status,
            string reason,
            string correlationId,
            CancellationToken cancellationToken)
        {
            var requestBody =
                new CustomerStatusSynchronizationRequest
                {
                    Status =
                        status.Trim(),


                    Reason =
                        reason.Trim()
                };


            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Put,
                    $"customers/" +
                    $"{Uri.EscapeDataString(authUserId.Trim())}" +
                    "/status",
                    correlationId);


            request.Content =
                JsonContent.Create(
                    requestBody);


            await SendAndEnsureSuccessAsync(
                request,
                "status synchronization",
                cancellationToken);
        }


        private HttpRequestMessage CreateAuthorizedRequest(
            HttpMethod method,
            string requestUri,
            string correlationId)
        {
            var token =
                _internalTokenService
                    .GenerateAuthServiceToken();


            var request =
                new HttpRequestMessage(
                    method,
                    requestUri);


            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);


            var effectiveCorrelationId =
                string.IsNullOrWhiteSpace(
                    correlationId)
                    ? Guid.NewGuid().ToString("N")
                    : correlationId.Trim();


            request.Headers.TryAddWithoutValidation(
                CorrelationIdHeader,
                effectiveCorrelationId);


            return request;
        }


        private async Task SendAndEnsureSuccessAsync(
            HttpRequestMessage request,
            string operation,
            CancellationToken cancellationToken)
        {
            using var response =
                await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption
                        .ResponseHeadersRead,
                    cancellationToken);


            if (response.IsSuccessStatusCode)
            {
                return;
            }


            var responseBody =
                response.Content is null
                    ? string.Empty
                    : await response.Content
                        .ReadAsStringAsync(
                            cancellationToken);


            throw new CustomerStatusSynchronizationException(
                $"Customer Service rejected {operation}. " +
                $"HTTP {(int)response.StatusCode}. " +
                $"{LimitResponseBody(responseBody)}",
                (int)response.StatusCode);
        }


        private static string LimitResponseBody(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }


            const int maximumLength = 500;


            return value.Length <= maximumLength
                ? value
                : value[..maximumLength];
        }
    }
}
