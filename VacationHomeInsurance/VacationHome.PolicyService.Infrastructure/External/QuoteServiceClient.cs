using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using VacationHome.PolicyService.Application.DTOs.External;
using VacationHome.PolicyService.Application.Exceptions;
using VacationHome.PolicyService.Application.Interfaces.External;


namespace VacationHome.PolicyService.Infrastructure.External


{
    public sealed class QuoteServiceClient
        : IQuoteServiceClient
    {
        private const string CorrelationIdHeader =
            "X-Correlation-ID";


        private readonly HttpClient _httpClient;


        private readonly IHttpContextAccessor
            _httpContextAccessor;


        public QuoteServiceClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient =
                httpClient;


            _httpContextAccessor =
                httpContextAccessor;
        }


        public async Task<ApprovedQuoteDto>
            GetApprovedQuoteAsync(
                Guid quoteId,
                Guid customerId,
                CancellationToken cancellationToken)
        {
            if (quoteId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Quote identifier is required.");
            }


            if (customerId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Customer identifier is required.");
            }


            var requestUri =
                $"internal/quotes/{quoteId}/validated" +
                $"?customerId={customerId}";


            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    requestUri);


            AddCorrelationId(request);


            using var response =
                await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);


            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException(
                    "Unable to retrieve the validated quote.");
            }


            return await response.Content
                       .ReadFromJsonAsync<ApprovedQuoteDto>(
                           cancellationToken:
                               cancellationToken)
                   ?? throw new BadRequestException(
                       "Quote Service returned an empty " +
                       "validated quote response.");
        }


        public async Task MarkQuoteConvertedAsync(
            Guid quoteId,
            Guid customerId,
            Guid policyId,
            Guid paymentId,
            decimal paidAmount,
            DateTime paymentSucceededAtUtc,
            string rowVersion,
            CancellationToken cancellationToken)
        {
            if (quoteId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Quote identifier is required.");
            }


            if (customerId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Customer identifier is required.");
            }


            if (policyId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Policy identifier is required.");
            }


            if (paymentId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Payment identifier is required.");
            }


            if (paidAmount <= 0)
            {
                throw new BadRequestException(
                    "Paid amount must be greater than zero.");
            }


            var correlationId =
                GetCorrelationId();


            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    $"internal/quotes/{quoteId}/converted")
                {
                    Content =
                        JsonContent.Create(
                            new
                            {
                                CustomerId = customerId,
                                PolicyId = policyId,
                                PaymentId = paymentId,
                                PaidAmount = paidAmount,
                                PaymentSucceededAtUtc =
                                    paymentSucceededAtUtc,
                                CorrelationId =
                                    correlationId,
                                RowVersion = rowVersion
                            })
                };


            AddCorrelationId(
                request,
                correlationId);


            using var response =
                await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);


            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException(
                    "Unable to mark the quote as converted.");
            }
        }


        public async Task<RenewalQuoteResultDto>
            CreateRenewalQuoteAsync(
                Guid policyId,
                Guid customerId,
                CancellationToken cancellationToken)
        {
            using var response =
                await _httpClient.PostAsJsonAsync(
                    "internal/quotes/renewals",
                    new
                    {
                        PolicyId = policyId,
                        CustomerId = customerId
                    },
                    cancellationToken);


            response.EnsureSuccessStatusCode();


            return await response.Content
                       .ReadFromJsonAsync<
                            RenewalQuoteResultDto>(
                            cancellationToken:
                                cancellationToken)
                   ?? throw new BadRequestException(
                        "Renewal quote response was empty.");
        }


        private string GetCorrelationId()
        {
            var correlationId =
                _httpContextAccessor
                    .HttpContext?
                    .TraceIdentifier;


            return string.IsNullOrWhiteSpace(
                correlationId)
                ? Guid.NewGuid().ToString("N")
                : correlationId.Trim();
        }


        private void AddCorrelationId(
            HttpRequestMessage request)
        {
            AddCorrelationId(
                request,
                GetCorrelationId());
        }


        private static void AddCorrelationId(
            HttpRequestMessage request,
            string correlationId)
        {
            request.Headers.TryAddWithoutValidation(
                CorrelationIdHeader,
                correlationId);
        }
    }
}
