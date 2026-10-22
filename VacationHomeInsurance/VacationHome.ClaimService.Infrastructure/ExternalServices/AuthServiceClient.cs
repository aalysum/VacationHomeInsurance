using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using VacationHome.ClaimService.Application.DTOs.External;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.External;


namespace VacationHome.ClaimService.Infrastructure
    .ExternalServices
{
    public sealed class AuthServiceClient
        : IAuthServiceClient
    {
        private readonly HttpClient
            _httpClient;


        private readonly ILogger<AuthServiceClient>
            _logger;


        public AuthServiceClient(
            HttpClient httpClient,
            ILogger<AuthServiceClient> logger)
        {
            _httpClient =
                httpClient;


            _logger =
                logger;
        }


        public async Task<ActiveClaimsManagerDto?>
            GetActiveClaimsManagerAsync(
                string claimsManagerId,
                CancellationToken cancellationToken = default)
        {
            if (
                string.IsNullOrWhiteSpace(
                    claimsManagerId)
            )
            {
                throw new BadRequestException(
                    "Claims Manager identifier is required.");
            }


            string normalizedClaimsManagerId =
                claimsManagerId.Trim();


            string encodedClaimsManagerId =
                Uri.EscapeDataString(
                    normalizedClaimsManagerId);


            string requestUri =
                $"internal/claims-managers/" +
                $"{encodedClaimsManagerId}/eligibility";


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
                    HttpStatusCode.BadRequest
                )
                {
                    _logger.LogWarning(
                        "Auth Service rejected Claims Manager " +
                        "identifier {ClaimsManagerId}.",
                        normalizedClaimsManagerId);


                    throw new BadRequestException(
                        "The selected Claims Manager identifier is invalid.");
                }


                if (
                    response.StatusCode ==
                    HttpStatusCode.Unauthorized
                )
                {
                    _logger.LogError(
                        "Auth Service rejected the Claim Service " +
                        "eligibility request as unauthorized. " +
                        "ClaimsManagerId: {ClaimsManagerId}.",
                        normalizedClaimsManagerId);


                    throw new UnauthorizedException(
                        "Claim Service could not authenticate with Auth Service.");
                }


                if (
                    response.StatusCode ==
                    HttpStatusCode.Forbidden
                )
                {
                    _logger.LogError(
                        "Auth Service denied the Claim Service " +
                        "eligibility request. ClaimsManagerId: " +
                        "{ClaimsManagerId}.",
                        normalizedClaimsManagerId);


                    throw new ForbiddenException(
                        "Claim Service is not authorized to verify Claims Manager eligibility.");
                }


                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Auth Service returned status code " +
                        "{StatusCode} while verifying Claims Manager " +
                        "{ClaimsManagerId}.",
                        (int)response.StatusCode,
                        normalizedClaimsManagerId);


                    throw new InvalidOperationException(
                        "Auth Service could not verify the selected Claims Manager.");
                }


                ActiveClaimsManagerDto?
                    claimsManager =
                        await response.Content
                            .ReadFromJsonAsync<
                                ActiveClaimsManagerDto>(
                                cancellationToken:
                                    cancellationToken);


                if (claimsManager is null)
                {
                    _logger.LogError(
                        "Auth Service returned an empty eligibility " +
                        "response for Claims Manager {ClaimsManagerId}.",
                        normalizedClaimsManagerId);


                    throw new InvalidOperationException(
                        "Auth Service returned an empty Claims Manager response.");
                }


                ValidateResponse(
                    normalizedClaimsManagerId,
                    claimsManager);


                return claimsManager;
            }
            catch (OperationCanceledException)
                when (!cancellationToken
                    .IsCancellationRequested)
            {
                _logger.LogError(
                    "Auth Service eligibility request timed out " +
                    "for Claims Manager {ClaimsManagerId}.",
                    normalizedClaimsManagerId);


                throw new InvalidOperationException(
                    "Auth Service did not respond in time.");
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    exception,
                    "Auth Service could not be reached while " +
                    "verifying Claims Manager {ClaimsManagerId}.",
                    normalizedClaimsManagerId);


                throw new InvalidOperationException(
                    "Auth Service could not be reached.",
                    exception);
            }
        }

        public async Task<InternalCustomerContactDto?>
            GetCustomerContactAsync(
                string userId,
                CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new BadRequestException(
                    "Customer user identifier is required.");
            }

            string requestUri =
                $"internal/users/{Uri.EscapeDataString(userId.Trim())}/customer-contact";

            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    requestUri,
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException(
                    "Claim Service could not authenticate with Auth Service.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new ForbiddenException(
                    "Claim Service is not authorized to retrieve customer contact details.");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Auth Service returned status code {StatusCode} while retrieving customer contact.",
                    (int)response.StatusCode);

                throw new InvalidOperationException(
                    "Auth Service could not retrieve customer contact details.");
            }

            return await response.Content.ReadFromJsonAsync<InternalCustomerContactDto>(
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException(
                    "Auth Service returned an empty customer contact response.");
        }


        private static void ValidateResponse(
            string requestedClaimsManagerId,
            ActiveClaimsManagerDto claimsManager)
        {
            if (
                string.IsNullOrWhiteSpace(
                    claimsManager.UserId)
            )
            {
                throw new InvalidOperationException(
                    "Auth Service returned a Claims Manager without a user identifier.");
            }


            if (
                !string.Equals(
                    claimsManager.UserId.Trim(),
                    requestedClaimsManagerId,
                    StringComparison.Ordinal)
            )
            {
                throw new InvalidOperationException(
                    "Auth Service returned an unexpected Claims Manager.");
            }


            if (
                string.IsNullOrWhiteSpace(
                    claimsManager.Email)
            )
            {
                throw new InvalidOperationException(
                    "Auth Service returned a Claims Manager without an email address.");
            }


            string displayName =
                GetDisplayName(
                    claimsManager);


            if (
                string.IsNullOrWhiteSpace(
                    displayName)
            )
            {
                throw new InvalidOperationException(
                    "Auth Service returned a Claims Manager without a display name.");
            }


            claimsManager.UserId =
                claimsManager.UserId.Trim();


            claimsManager.FirstName =
                claimsManager.FirstName.Trim();


            claimsManager.LastName =
                claimsManager.LastName.Trim();


            claimsManager.Email =
                claimsManager.Email.Trim();


            claimsManager.ProfessionalSummary =
                claimsManager
                    .ProfessionalSummary
                    .Trim();


            claimsManager.DisplayName =
                displayName;
        }


        private static string GetDisplayName(
            ActiveClaimsManagerDto claimsManager)
        {
            string generatedDisplayName =
                string.Join(
                    " ",
                    new[]
                    {
                        claimsManager.FirstName,
                        claimsManager.LastName
                    }
                    .Where(
                        value =>
                            !string.IsNullOrWhiteSpace(
                                value))
                    .Select(
                        value =>
                            value.Trim()));


            if (
                !string.IsNullOrWhiteSpace(
                    generatedDisplayName)
            )
            {
                return generatedDisplayName;
            }


            if (
                !string.IsNullOrWhiteSpace(
                    claimsManager.DisplayName)
            )
            {
                return claimsManager
                    .DisplayName
                    .Trim();
            }


            return string.Empty;
        }
    }
}
