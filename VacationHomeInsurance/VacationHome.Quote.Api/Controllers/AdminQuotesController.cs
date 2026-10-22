using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Admin;
using VacationHome.ProductQuote.Application.DTOs.Quotes;
using VacationHome.ProductQuote.Application.Interfaces.Services;


namespace VacationHome.ProductQuote.API.Controllers
{
    [ApiController]
    [Route("admin/quotes")]
    [Authorize(
        Policy =
            ProductQuoteAuthorizationPolicies.AdminOnly)]
    public sealed class AdminQuotesController : ControllerBase
    {
        private readonly IQuoteAdminService
            _quoteAdminService;


        private readonly IQuoteQueryService
            _quoteQueryService;


        private readonly IQuoteLifecycleService
            _quoteLifecycleService;


        public AdminQuotesController(
            IQuoteAdminService quoteAdminService,
            IQuoteQueryService quoteQueryService,
            IQuoteLifecycleService quoteLifecycleService)
        {
            _quoteAdminService = quoteAdminService;
            _quoteQueryService = quoteQueryService;
            _quoteLifecycleService =
                quoteLifecycleService;
        }


        [HttpGet]
        [ProducesResponseType(
            typeof(PagedResult<QuoteSummaryDto>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<
            PagedResult<QuoteSummaryDto>>>
            SearchQuotesAsync(
                [FromQuery] AdminQuoteSearchRequest request,
                CancellationToken cancellationToken)
        {
            PagedResult<QuoteSummaryDto> result =
                await _quoteQueryService
                    .SearchAdminQuotesAsync(
                        request,
                        cancellationToken);


            return Ok(result);
        }


        [HttpPost("{quoteId:guid}/approve")]
        [ProducesResponseType(
            typeof(QuoteDetailsDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<QuoteDetailsDto>>
            ApproveAsync(
                Guid quoteId,
                [FromBody] ApproveQuoteRequest request,
                CancellationToken cancellationToken)
        {
            QuoteDetailsDto quote =
                await _quoteAdminService.ApproveAsync(
                    quoteId,
                    request,
                    cancellationToken);


            return Ok(quote);
        }


        [HttpPost("{quoteId:guid}/reject")]
        [ProducesResponseType(
            typeof(QuoteDetailsDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<QuoteDetailsDto>>
            RejectAsync(
                Guid quoteId,
                [FromBody] AdminRejectionRequest request,
                CancellationToken cancellationToken)
        {
            QuoteDetailsDto quote =
                await _quoteAdminService.RejectAsync(
                    quoteId,
                    request,
                    cancellationToken);


            return Ok(quote);
        }


        [HttpGet("{quoteId:guid}")]
        [ProducesResponseType(
    typeof(QuoteDetailsDto),
    StatusCodes.Status200OK)]
        [ProducesResponseType(
    StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
    StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
    StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuoteDetailsDto>>
    GetQuoteAsync(
        Guid quoteId,
        CancellationToken cancellationToken)
        {
            QuoteDetailsDto quote =
                await _quoteQueryService
                    .GetAdminQuoteAsync(
                        quoteId,
                        cancellationToken);


            return Ok(quote);
        }


        [HttpPost("{quoteId:guid}/invalidate")]
        [ProducesResponseType(
            typeof(QuoteDetailsDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<QuoteDetailsDto>>
            InvalidateAsync(
                Guid quoteId,
                [FromBody] InvalidateQuoteRequest request,
                CancellationToken cancellationToken)
        {
            string actorId =
                User.FindFirst("sub")?.Value ??
                User.FindFirst(
                    System.Security.Claims
                        .ClaimTypes.NameIdentifier)
                    ?.Value ??
                "Unknown";


            QuoteDetailsDto quote =
                await _quoteLifecycleService
                    .InvalidateQuoteAsync(
                        quoteId,
                        request.Reason,
                        actorId,
                        "Admin",
                        cancellationToken);


            return Ok(quote);
        }
    }






    public sealed class InvalidateQuoteRequest
    {
        public string Reason { get; init; } =
            string.Empty;
    }
}
