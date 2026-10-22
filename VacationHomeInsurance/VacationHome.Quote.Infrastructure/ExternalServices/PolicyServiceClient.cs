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
    public sealed class PolicyServiceClient :
        IPolicyServiceClient
    {
        private static readonly JsonSerializerOptions
            JsonOptions = new(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };


        private readonly HttpClient _httpClient;
        private readonly PolicyServiceOptions _options;
        private readonly ICorrelationContext _correlationContext;
        private readonly ILogger<PolicyServiceClient> _logger;


        public PolicyServiceClient(
            HttpClient httpClient,
            IOptions<PolicyServiceOptions> options,
            ICorrelationContext correlationContext,
            ILogger<PolicyServiceClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _correlationContext = correlationContext;
            _logger = logger;
        }


        public async Task<PolicyOverlapResult> CheckOverlapAsync(
            PolicyOverlapRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            ValidateRequest(request);


            using HttpRequestMessage httpRequest =
                CreateRequest(request);


            using HttpResponseMessage response =
                await SendAsync(
                    httpRequest,
                    cancellationToken);


            await EnsureSuccessAsync(
                response,
                cancellationToken);


            return await ReadRequiredResponseAsync(
                response,
                cancellationToken);
        }


        public async Task<InternalPolicyDto?> GetPolicyAsync(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            if (policyId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Policy identifier is required.",
                    nameof(policyId));
            }


            using HttpRequestMessage request =
                new(
                    HttpMethod.Get,
                    $"internal/policies/{policyId}");


            string correlationId =
                _correlationContext.CorrelationId;


            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                request.Headers.TryAddWithoutValidation(
                    "X-Correlation-ID",
                    correlationId);
            }


            using HttpResponseMessage response =
                await SendAsync(
                    request,
                    cancellationToken);


            if (response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return null;
            }


            await EnsureSuccessAsync(
                response,
                cancellationToken);


            return await response.Content
                       .ReadFromJsonAsync<InternalPolicyDto>(
                           cancellationToken:
                               cancellationToken);
        }




        private HttpRequestMessage CreateRequest(
            PolicyOverlapRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    _options.CheckOverlapPath))
            {
                throw new InvalidOperationException(
                    "Policy Service overlap endpoint is not configured.");
            }


            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                _options.CheckOverlapPath)
            {
                Content = JsonContent.Create(
                    request,
                    options: JsonOptions)
            };


            string correlationId =
                _correlationContext.CorrelationId;


            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                httpRequest.Headers.TryAddWithoutValidation(
                    "X-Correlation-ID",
                    correlationId);
            }


            return httpRequest;
        }


        private async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
            }
            catch (OperationCanceledException exception)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    exception,
                    "Policy Service timed out while checking Product overlap.");


                throw new DependencyUnavailableException(
                    "PolicyService",
                    "Policy Service timed out while checking Product overlap.");
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    exception,
                    "Policy Service was unavailable while checking Product overlap.");


                throw new DependencyUnavailableException(
                    "PolicyService",
                    "Policy Service was unavailable while checking Product overlap.");
            }
        }


        private static async Task EnsureSuccessAsync(
            HttpResponseMessage response,
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
                    ? "POLICY_OVERLAP_REQUEST_FAILED"
                    : error.ErrorCode;


            string message =
                string.IsNullOrWhiteSpace(error?.Message)
                    ? "Product overlap validation could not be completed."
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
                        "PolicyService",
                        message);


                default:
                    throw new DependencyUnavailableException(
                        "PolicyService",
                        $"Policy Service returned HTTP status {(int)response.StatusCode}. {message}");
            }
        }


        private static async Task<PolicyOverlapResult>
            ReadRequiredResponseAsync(
                HttpResponseMessage response,
                CancellationToken cancellationToken)
        {
            string content =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);


            if (string.IsNullOrWhiteSpace(content))
            {
                throw new DependencyUnavailableException(
                    "PolicyService",
                    "Policy Service returned an empty Product-overlap response.");
            }


            try
            {
                ApiResponse<PolicyOverlapResult>?
                    wrappedResponse =
                        JsonSerializer.Deserialize<
                            ApiResponse<PolicyOverlapResult>>(
                            content,
                            JsonOptions);


                if (wrappedResponse?.Data is not null)
                {
                    return wrappedResponse.Data;
                }


                PolicyOverlapResult? directResponse =
                    JsonSerializer.Deserialize<
                        PolicyOverlapResult>(
                        content,
                        JsonOptions);


                return directResponse
                    ?? throw new DependencyUnavailableException(
                        "PolicyService",
                        "Policy Service returned an empty Product-overlap result.");
            }
            catch (JsonException exception)
            {
                throw new DependencyUnavailableException(
                    "PolicyService",
                    $"Policy Service returned an invalid Product-overlap response: {exception.Message}");
            }
        }


        private static async Task<ErrorResponse?>
            TryReadErrorAsync(
                HttpResponseMessage response,
                CancellationToken cancellationToken)
        {
            string content =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);


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


        private static void ValidateRequest(
            PolicyOverlapRequest request)
        {
            if (request.CustomerId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Customer identifier is required.",
                    nameof(request));
            }


            if (request.PropertyId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Property identifier is required.",
                    nameof(request));
            }


            if (!Enum.IsDefined(request.ProductType))
            {
                throw new ArgumentException(
                    "Product type is invalid.",
                    nameof(request));
            }
        }


        private sealed class ErrorResponse
        {
            public string? ErrorCode { get; init; }


            public string? Message { get; init; }
        }
    }
}
