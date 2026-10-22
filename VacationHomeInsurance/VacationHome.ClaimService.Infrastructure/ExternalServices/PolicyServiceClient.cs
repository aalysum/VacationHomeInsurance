using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using VacationHome.ClaimService.Application.DTOs.External;
using VacationHome.ClaimService.Application.DTOs.Documents;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.External;


namespace VacationHome.ClaimService.Infrastructure.ExternalServices
{
    public sealed class PolicyServiceClient
        : IPolicyServiceClient
    {
        private readonly HttpClient
            _httpClient;


        private readonly ILogger<PolicyServiceClient>
            _logger;


        public PolicyServiceClient(
            HttpClient httpClient,
            ILogger<PolicyServiceClient> logger)
        {
            _httpClient =
                httpClient;


            _logger =
                logger;
        }


        public async Task<PolicyDetailsDto?>
            GetPolicyAsync(
                Guid policyId,
                CancellationToken cancellationToken = default)
        {
            if (policyId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Policy identifier is required.");
            }


            string requestUri =
                $"internal/policies/{policyId}";


            try
            {
                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Get,
                        requestUri);


                using HttpResponseMessage response =
                    await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption
                            .ResponseHeadersRead,
                        cancellationToken);


                if (
                    response.StatusCode ==
                    HttpStatusCode.NotFound
                )
                {
                    return null;
                }


                if (
                    response.StatusCode ==
                    HttpStatusCode.Unauthorized
                )
                {
                    _logger.LogWarning(
                        "Policy Service rejected the internal " +
                        "request for Policy {PolicyId} as unauthorized.",
                        policyId);


                    throw new UnauthorizedException(
                        "Policy Service authentication failed.");
                }


                if (
                    response.StatusCode ==
                    HttpStatusCode.Forbidden
                )
                {
                    _logger.LogWarning(
                        "Policy Service rejected the internal " +
                        "request for Policy {PolicyId} as forbidden.",
                        policyId);


                    throw new ForbiddenException(
                        "Claim Service is not authorized to retrieve Policy details.");
                }


                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Policy Service returned status code " +
                        "{StatusCode} while retrieving Policy {PolicyId}.",
                        (int)response.StatusCode,
                        policyId);


                    throw new InvalidOperationException(
                        "Policy Service could not retrieve the requested Policy.");
                }


                PolicyDetailsDto? policy =
                    await response.Content
                        .ReadFromJsonAsync<
                            PolicyDetailsDto>(
                            cancellationToken:
                                cancellationToken);


                if (policy is null)
                {
                    _logger.LogError(
                        "Policy Service returned an empty response " +
                        "for Policy {PolicyId}.",
                        policyId);


                    throw new InvalidOperationException(
                        "Policy Service returned an empty Policy response.");
                }


                if (policy.Id != policyId)
                {
                    _logger.LogError(
                        "Policy Service returned Policy {ReturnedPolicyId} " +
                        "when Policy {RequestedPolicyId} was requested.",
                        policy.Id,
                        policyId);


                    throw new InvalidOperationException(
                        "Policy Service returned an unexpected Policy.");
                }


                if (policy.CustomerId == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "Policy Service returned a Policy without a Customer identifier.");
                }


                if (policy.PropertyId == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "Policy Service returned a Policy without a Property identifier.");
                }


                if (policy.Coverages is null)
                {
                    throw new InvalidOperationException(
                        "Policy Service returned a Policy without a Coverage collection.");
                }


                bool hasInvalidCoverage =
                    policy.Coverages.Any(
                        coverage =>
                            coverage.CoverageId ==
                                Guid.Empty ||
                            string.IsNullOrWhiteSpace(
                                coverage.CoverageName) ||
                            coverage.CoverageLimitAmount <
                                0 ||
                            coverage.CoverageLimitPercentage <
                                0 ||
                            coverage.CoverageCharge <
                                0);


                if (hasInvalidCoverage)
                {
                    throw new InvalidOperationException(
                        "Policy Service returned an invalid Policy Coverage.");
                }


                bool hasDuplicateCoverage =
                    policy.Coverages
                        .GroupBy(
                            coverage =>
                                coverage.CoverageId)
                        .Any(
                            group =>
                                group.Count() > 1);


                if (hasDuplicateCoverage)
                {
                    throw new InvalidOperationException(
                        "Policy Service returned duplicate Policy Coverages.");
                }


                return policy;
            }
            catch (OperationCanceledException)
                when (!cancellationToken
                    .IsCancellationRequested)
            {
                _logger.LogError(
                    "The Policy Service request timed out while " +
                    "retrieving Policy {PolicyId}.",
                    policyId);


                throw new InvalidOperationException(
                    "Policy Service did not respond in time.");
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    exception,
                    "Policy Service could not be reached while " +
                    "retrieving Policy {PolicyId}.",
                    policyId);


                throw new InvalidOperationException(
                    "Policy Service could not be reached.",
                    exception);
            }
        }

        public async Task<PolicyDocumentContentDto?>
            DownloadPolicyDocumentAsync(
                Guid policyId,
                Guid documentId,
                CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    $"internal/policies/{policyId}/documents/{documentId}/download",
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException(
                    "Policy Service authentication failed.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new ForbiddenException(
                    "Claim Service is not authorized to retrieve this policy document.");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Policy Service returned status code {StatusCode} while downloading document {DocumentId} for policy {PolicyId}.",
                    (int)response.StatusCode,
                    documentId,
                    policyId);

                throw new InvalidOperationException(
                    "Policy Service could not retrieve the policy document.");
            }

            byte[] content =
                await response.Content.ReadAsByteArrayAsync(
                    cancellationToken);
            string fileName =
                response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName
                ?? $"policy-document-{documentId}.pdf";

            return new PolicyDocumentContentDto(
                content,
                Path.GetFileName(fileName.Trim('"')),
                response.Content.Headers.ContentType?.MediaType
                    ?? "application/pdf");
        }
    }
}
