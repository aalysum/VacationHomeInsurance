using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.ProductQuote.Application.DTOs.Internal;
using VacationHome.ProductQuote.Application.Interfaces.Services;


namespace VacationHome.ProductQuote.API.Controllers
{
    [ApiController]
    [Route("internal/quotes")]
    [Authorize(Policy = "PolicyServiceOnly")]
    public sealed class InternalQuotesController
        : ControllerBase
    {
        private readonly IQuoteIntegrationService
            _integrationService;


        public InternalQuotesController(
            IQuoteIntegrationService integrationService)
        {
            _integrationService = integrationService;
        }


        [HttpPost("validate-for-payment")]
        [ProducesResponseType(
            typeof(ValidatedQuoteDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<ValidatedQuoteDto>>
            ValidateForPaymentAsync(
                [FromBody]
                    ValidateQuoteForPaymentRequest request,
                CancellationToken cancellationToken)
        {
            ValidatedQuoteDto result =
                await _integrationService
                    .ValidateQuoteForPaymentAsync(
                        request,
                        cancellationToken);


            return Ok(result);
        }


        [HttpGet("{quoteId:guid}/validated")]
        [ProducesResponseType(
            typeof(ValidatedQuoteDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<ValidatedQuoteDto>>
            GetValidatedQuoteAsync(
                Guid quoteId,
                [FromQuery] Guid customerId,
                CancellationToken cancellationToken)
        {
            ValidatedQuoteDto result =
                await _integrationService
                    .GetValidatedQuoteAsync(
                        quoteId,
                        customerId,
                        HttpContext.TraceIdentifier,
                        cancellationToken);


            return Ok(result);
        }


        [HttpPost("{quoteId:guid}/converted")]
        [ProducesResponseType(
            typeof(QuoteConversionResultDto),
            StatusCodes.Status200OK)]
        public async Task<
            ActionResult<QuoteConversionResultDto>>
            MarkConvertedAsync(
                Guid quoteId,
                [FromBody]
                    MarkQuoteConvertedRequest request,
                CancellationToken cancellationToken)
        {
            QuoteConversionResultDto result =
                await _integrationService
                    .MarkQuoteConvertedAsync(
                        quoteId,
                        request,
                        cancellationToken);


            return Ok(result);
        }


        [HttpPost("renewals")]
        [ProducesResponseType(
            typeof(RenewalQuoteResultDto),
            StatusCodes.Status200OK)]
        public async Task<
            ActionResult<RenewalQuoteResultDto>>
            CreateRenewalQuoteAsync(
                [FromBody]
        CreateRenewalQuoteRequest request,
                CancellationToken cancellationToken)
        {
            var result =
                await _integrationService
                    .CreateRenewalQuoteAsync(
                        request,
                        cancellationToken);


            return Ok(result);
        }


    }
}
