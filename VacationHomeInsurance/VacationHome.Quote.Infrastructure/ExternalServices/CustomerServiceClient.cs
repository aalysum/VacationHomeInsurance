using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.Common.Models;
using VacationHome.ProductQuote.Application.DTOs.External;
using VacationHome.ProductQuote.Application.Exceptions;
using VacationHome.ProductQuote.Application.Interfaces.External;
using VacationHome.ProductQuote.Application.Interfaces.Services;


namespace VacationHome.ProductQuote.Infrastructure.ExternalServices
{
    public sealed class CustomerServiceClient :
        ICustomerServiceClient
    {
        private static readonly JsonSerializerOptions
            JsonOptions = new(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };


        private readonly HttpClient _httpClient;
        private readonly CustomerServiceOptions _options;
        private readonly ICorrelationContext _correlationContext;
        private readonly ILogger<CustomerServiceClient> _logger;


        public CustomerServiceClient(
            HttpClient httpClient,
            IOptions<CustomerServiceOptions> options,
            ICorrelationContext correlationContext,
            ILogger<CustomerServiceClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _correlationContext = correlationContext;
            _logger = logger;
        }


        public async Task<CustomerStatusDto>
            GetCustomerStatusAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            if (customerId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Customer identifier is required.",
                    nameof(customerId));
            }


            string relativePath = BuildPath(
                _options.CustomerStatusPathTemplate,
                "customerId",
                customerId);


            using HttpRequestMessage request =
                CreateGetRequest(relativePath);


            using HttpResponseMessage response =
                await SendAsync(
                    request,
                    "retrieve Customer status",
                    cancellationToken);


            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException(
                    "CUSTOMER_NOT_FOUND",
                    "Customer was not found.");
            }


            await EnsureSuccessAsync(
                response,
                "CUSTOMER_STATUS_REQUEST_FAILED",
                "Customer status could not be retrieved.",
                cancellationToken);


            return await ReadRequiredResponseAsync<CustomerStatusDto>(
                response,
                "Customer Service returned an empty Customer status response.",
                cancellationToken);
        }


        public async Task<PropertyDetailsDto?> GetPropertyAsync(
            Guid propertyId,
            CancellationToken cancellationToken)
        {
            if (propertyId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Property identifier is required.",
                    nameof(propertyId));
            }


            string relativePath = BuildPath(
                _options.PropertyPathTemplate,
                "propertyId",
                propertyId);


            using HttpRequestMessage request =
                CreateGetRequest(relativePath);


            using HttpResponseMessage response =
                await SendAsync(
                    request,
                    "retrieve Property details",
                    cancellationToken);


            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }


            await EnsureSuccessAsync(
                response,
                "PROPERTY_REQUEST_FAILED",
                "Property details could not be retrieved.",
                cancellationToken);


            return await ReadRequiredResponseAsync<PropertyDetailsDto>(
                response,
                "Customer Service returned an empty Property response.",
                cancellationToken);
        }


        private HttpRequestMessage CreateGetRequest(
            string relativePath)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                relativePath);


            string correlationId =
                _correlationContext.CorrelationId;


            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                request.Headers.TryAddWithoutValidation(
                    "X-Correlation-ID",
                    correlationId);
            }


            return request;
        }


        private async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            string operation,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    "Customer Service timed out while attempting to {Operation}.",
                    operation);


                throw new DependencyUnavailableException(
                    "CustomerService",
                    $"Customer Service timed out while attempting to {operation}.");
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    exception,
                    "Customer Service was unavailable while attempting to {Operation}.",
                    operation);


                throw new DependencyUnavailableException(
                    "CustomerService",
                    $"Customer Service was unavailable while attempting to {operation}.");
            }
        }


        private static async Task EnsureSuccessAsync(
            HttpResponseMessage response,
            string fallbackErrorCode,
            string fallbackMessage,
            CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }


            ErrorResponse? error =
                await TryReadErrorAsync(
                    response,
                    cancellationToken);


            string errorCode =
                string.IsNullOrWhiteSpace(error?.ErrorCode)
                    ? fallbackErrorCode
                    : error.ErrorCode;


            string message =
                string.IsNullOrWhiteSpace(error?.Message)
                    ? fallbackMessage
                    : error.Message;


            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new BadRequestException(
                        errorCode,
                        message);


                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedException(message);


                case HttpStatusCode.Forbidden:
                    throw new ForbiddenException(message);


                case HttpStatusCode.NotFound:
                    throw new NotFoundException(
                        errorCode,
                        message);


                case HttpStatusCode.Conflict:
                    throw new ConflictException(
                        errorCode,
                        message);


                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.GatewayTimeout:
                    throw new DependencyUnavailableException(
                        "CustomerService",
                        message);


                default:
                    throw new DependencyUnavailableException(
                        "CustomerService",
                        $"{message} Customer Service returned HTTP status {(int)response.StatusCode}.");
            }
        }


        private static async Task<T> ReadRequiredResponseAsync<T>(
            HttpResponseMessage response,
            string emptyResponseMessage,
            CancellationToken cancellationToken)
        {
            string content = await response.Content
                .ReadAsStringAsync(cancellationToken);


            if (string.IsNullOrWhiteSpace(content))
            {
                throw new DependencyUnavailableException(
                    "CustomerService",
                    emptyResponseMessage);
            }


            try
            {
                ApiResponse<T>? wrappedResponse =
                    JsonSerializer.Deserialize<ApiResponse<T>>(
                        content,
                        JsonOptions);
                if (wrappedResponse is { Data: not null })
                {
                    return wrappedResponse.Data;
                }
                T? directResponse =
                    JsonSerializer.Deserialize<T>(
                        content,
                        JsonOptions);


                return directResponse
                    ?? throw new DependencyUnavailableException(
                        "CustomerService",
                        emptyResponseMessage);
            }
            catch (JsonException exception)
            {
                throw new DependencyUnavailableException(
                    "CustomerService",
                    $"Customer Service returned an invalid response: {exception.Message}");
            }
        }


        private static async Task<ErrorResponse?>
            TryReadErrorAsync(
                HttpResponseMessage response,
                CancellationToken cancellationToken)
        {
            string content = await response.Content
                .ReadAsStringAsync(cancellationToken);


            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }


            try
            {
                ErrorResponse? directError =
                    JsonSerializer.Deserialize<ErrorResponse>(
                        content,
                        JsonOptions);


                if (!string.IsNullOrWhiteSpace(
                        directError?.Message))
                {
                    return directError;
                }


                ApiResponse<object>? wrappedError =
                    JsonSerializer.Deserialize<
                        ApiResponse<object>>(
                        content,
                        JsonOptions);


                if (wrappedError is null)
                {
                    return null;
                }


                return new ErrorResponse
                { 
                    Message = wrappedError.Message
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }


        private static string BuildPath(
            string pathTemplate,
            string parameterName,
            Guid parameterValue)
        {
            if (string.IsNullOrWhiteSpace(pathTemplate))
            {
                throw new InvalidOperationException(
                    "Customer Service endpoint path is not configured.");
            }


            return pathTemplate.Replace(
                $"{{{parameterName}}}",
                Uri.EscapeDataString(
                    parameterValue.ToString()),
                StringComparison.OrdinalIgnoreCase);
        }


        private sealed class ErrorResponse
        {
            public string? ErrorCode { get; init; }


            public string? Message { get; init; }
        }
    }
}
