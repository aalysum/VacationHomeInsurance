using Microsoft.Extensions.Logging;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Admin;
using VacationHome.ProductQuote.Application.DTOs.External;
using VacationHome.ProductQuote.Application.DTOs.Quotes;
using VacationHome.ProductQuote.Application.Exceptions;
using VacationHome.ProductQuote.Application.Interfaces.External;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Application.Interfaces.Services;
using VacationHome.ProductQuote.Application.Mappers;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Services
{
    public sealed class QuoteQueryService : IQuoteQueryService
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICustomerServiceClient
            _customerServiceClient;
        private readonly IPolicyServiceClient _policyServiceClient;
        private readonly ILogger<QuoteQueryService> _logger;


        public QuoteQueryService(
            IQuoteRepository quoteRepository,
            ICurrentUserService currentUserService,
            ICustomerServiceClient customerServiceClient,
            IPolicyServiceClient policyServiceClient,
            ILogger<QuoteQueryService> logger)
        {
            _quoteRepository =
                quoteRepository;


            _currentUserService =
                currentUserService;


            _customerServiceClient =
                customerServiceClient;
            _policyServiceClient = policyServiceClient;
            _logger = logger;
        }




        public async Task<QuoteDetailsDto> GetMyQuoteAsync(
            Guid quoteId,
            CancellationToken cancellationToken)
        {
            Guid customerId =
                await ResolveCustomerIdAsync(
                    cancellationToken);


            if (quoteId == Guid.Empty)
            {
                throw new BadRequestException(
                    "INVALID_QUOTE_ID",
                    "Quote identifier is required.");
            }


            Quote quote =
                await _quoteRepository.GetDetailsAsync(
                    quoteId,
                    tracking: false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");


            if (quote.CustomerId != customerId)
            {
                /*
                 * Return NotFound rather than Forbidden so
                 * another Customer cannot determine whether
                 * the Quote identifier exists.
                 */
                throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");
            }


            DateTime utcNow = DateTime.UtcNow;


            QuoteDetailsDto details = quote.ToDetailsDto(utcNow);
            try
            {
                PolicyOverlapResult overlap =
                    await _policyServiceClient.CheckOverlapAsync(
                        new PolicyOverlapRequest(
                            quote.CustomerId,
                            quote.PropertyId,
                            quote.ProductType,
                            quote.Id),
                        cancellationToken);

                if (overlap.HasConflict)
                {
                    string conflict =
                        string.IsNullOrWhiteSpace(overlap.Message)
                            ? "An existing policy conflicts with this protection."
                            : overlap.Message;
                    details.PolicyConversionWarning =
                        $"{conflict} You may still accept and submit this quote, " +
                        "but it cannot be approved or converted to a policy while " +
                        "the conflicting policy remains active.";
                }
            }
            catch (DependencyUnavailableException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Unable to check policy conversion eligibility for customer quote {QuoteId}.",
                    quote.Id);
            }

            return details;
        }


        public async Task<PagedResult<QuoteSummaryDto>>
            SearchMyQuotesAsync(
                QuoteSearchRequest request,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            Guid customerId =
                await ResolveCustomerIdAsync(
                    cancellationToken);


            ValidatePagination(
                request.PageNumber,
                request.PageSize);


            PagedResult<Quote> result =
                await _quoteRepository
                    .SearchCustomerQuotesAsync(
                        customerId,
                        request,
                        cancellationToken);


            QuoteSummaryDto[] mappedItems =
                result.Items
                    .Select(quote =>
                        quote.ToSummaryDto())
                    .ToArray();


            return new PagedResult<QuoteSummaryDto>
            {
                Items = mappedItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }


        public async Task<PagedResult<QuoteSummaryDto>>
            SearchAdminQuotesAsync(
                AdminQuoteSearchRequest request,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            RequireAdmin();


            ValidatePagination(
                request.PageNumber,
                request.PageSize);


            PagedResult<Quote> result =
                await _quoteRepository
                    .SearchAdminQuotesAsync(
                        request,
                        cancellationToken);


            QuoteSummaryDto[] mappedItems =
                result.Items
                    .Select(quote =>
                        quote.ToSummaryDto())
                    .ToArray();


            return new PagedResult<QuoteSummaryDto>
            {
                Items = mappedItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }


        public async Task<QuoteDetailsDto> GetAdminQuoteAsync(
            Guid quoteId,
            CancellationToken cancellationToken)
        {
            RequireAdmin();


            if (quoteId == Guid.Empty)
            {
                throw new BadRequestException(
                    "INVALID_QUOTE_ID",
                    "Quote identifier is required.");
            }


            Quote quote =
                await _quoteRepository.GetDetailsAsync(
                    quoteId,
                    tracking: false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "QUOTE_NOT_FOUND",
                    "Quote was not found.");


            return quote.ToDetailsDto(
                DateTime.UtcNow);
        }


        private Guid RequireCustomer()
        {
            EnsureAuthenticated();


            if (!string.Equals(
                    _currentUserService.Role,
                    "Customer",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenException(
                    "Customer role is required.");
            }


            return _currentUserService.UserId!.Value;
        }


        private Guid RequireAdmin()
        {
            EnsureAuthenticated();


            if (!string.Equals(
                    _currentUserService.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenException(
                    "Admin role is required.");
            }


            return _currentUserService.UserId!.Value;
        }


        private void EnsureAuthenticated()
        {
            if (!_currentUserService.IsAuthenticated ||
                !_currentUserService.UserId.HasValue)
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
            }
        }


        private static void ValidatePagination(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new BadRequestException(
                    "INVALID_PAGE_NUMBER",
                    "Page number must be greater than zero.");
            }


            if (!PaginationConstants.AllowedPageSizes
                    .Contains(pageSize))
            {
                throw new BadRequestException(
                    "INVALID_PAGE_SIZE",
                    "Page size must be 10, 25, or 50.");
            }


        }


        private async Task<Guid>
            ResolveCustomerIdAsync(
                CancellationToken cancellationToken)
        {
            Guid authUserId =
                RequireCustomer();


            CustomerStatusDto customerStatus =
                await _customerServiceClient
                    .GetCustomerStatusAsync(
                        authUserId,
                        cancellationToken);


            if (
                !Guid.TryParse(
                    customerStatus.AuthUserId,
                    out Guid returnedAuthUserId) ||
                returnedAuthUserId != authUserId)
            {
                throw new ConflictException(
                    "CUSTOMER_ID_MISMATCH",
                    "Customer Service returned a different authentication user identifier.");
            }


            if (!customerStatus.IsActive)
            {
                throw new ForbiddenException(
                    "The Customer account is inactive.");
            }


            return customerStatus.CustomerId;
        }
    }
}
