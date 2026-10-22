using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Quotes;
using VacationHome.ProductQuote.Application.Interfaces.Services;


namespace VacationHome.ProductQuote.API.Controllers
{
    [ApiController]
    [Route("quotes")]
    [Authorize(
        Policy =
            ProductQuoteAuthorizationPolicies
                .CustomerOnly)]
    public sealed class QuotesController
        : ControllerBase
    {
        private readonly IQuoteGenerationService
            _generationService;


        private readonly IQuoteLifecycleService
            _lifecycleService;


        private readonly IQuoteQueryService
            _queryService;


        public QuotesController(
            IQuoteGenerationService generationService,
            IQuoteLifecycleService lifecycleService,
            IQuoteQueryService queryService)
        {
            _generationService =
                generationService;


            _lifecycleService =
                lifecycleService;


            _queryService =
                queryService;
        }


        [HttpPost]
        [ProducesResponseType(
           typeof(QuoteDetailsDto),
           StatusCodes.Status201Created)]
        [ProducesResponseType(
           StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
           StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
           StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
           StatusCodes.Status404NotFound)]
        [ProducesResponseType(
           StatusCodes.Status409Conflict)]
        public async Task<ActionResult<QuoteDetailsDto>>
           GenerateQuoteAsync(
               [FromBody]
            GenerateQuoteRequest request,
               CancellationToken cancellationToken)
        {
            QuoteDetailsDto result =
                await _generationService
                    .GenerateAsync(
                        request,
                        cancellationToken);


            return StatusCode(
                StatusCodes.Status201Created,
                result);
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
        public async Task<
            ActionResult<QuoteDetailsDto>>
            GetMyQuoteAsync(
                [FromRoute]
                    Guid quoteId,
                CancellationToken cancellationToken)
        {
            QuoteDetailsDto quote =
                await _queryService
                    .GetMyQuoteAsync(
                        quoteId,
                        cancellationToken);


            return Ok(
                quote);
        }


        [HttpGet]
        [ProducesResponseType(
            typeof(
                PagedResult<QuoteSummaryDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<
            ActionResult<
                PagedResult<QuoteSummaryDto>>>
            SearchMyQuotesAsync(
                [FromQuery]
                    QuoteSearchRequest request,
                CancellationToken cancellationToken)
        {
            PagedResult<QuoteSummaryDto> result =
                await _queryService
                    .SearchMyQuotesAsync(
                        request,
                        cancellationToken);


            return Ok(
                result);
        }


        [HttpPost(
            "{quoteId:guid}/accept")]
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
        [ProducesResponseType(
            StatusCodes.Status409Conflict)]
        public async Task<
            ActionResult<QuoteDetailsDto>>
            AcceptQuoteAsync(
                [FromRoute]
                    Guid quoteId,
                [FromBody]
                    AcceptQuoteRequest request,
                CancellationToken cancellationToken)
        {
            QuoteDetailsDto quote =
                await _lifecycleService
                    .AcceptQuoteAsync(
                        quoteId,
                        request,
                        cancellationToken);


            return Ok(
                quote);
        }


        [HttpPost(
            "{quoteId:guid}/withdraw")]
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
        [ProducesResponseType(
            StatusCodes.Status409Conflict)]
        public async Task<
            ActionResult<QuoteDetailsDto>>
            WithdrawQuoteAsync(
                [FromRoute]
                    Guid quoteId,
                [FromBody]
                    WithdrawQuoteRequest request,
                CancellationToken cancellationToken)
        {
            QuoteDetailsDto quote =
                await _lifecycleService
                    .WithdrawQuoteAsync(
                        quoteId,
                        request,
                        cancellationToken);


            return Ok(
                quote);
        }
    }
}
