using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Pricing;
using VacationHome.ProductQuote.Application.DTOs.Products;
using VacationHome.ProductQuote.Application.Interfaces.Services;


namespace VacationHome.ProductQuote.API.Controllers
{
    [ApiController]
    [Route("admin")]
    [Authorize(
        Policy =
            ProductQuoteAuthorizationPolicies.AdminOnly)]
    public sealed class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;


        public AdminProductsController(
            IProductService productService)
        {
            _productService = productService;
        }


        [HttpPut("products/{productId:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult>
            UpdateProductStatusAsync(
                Guid productId,
                [FromBody] UpdateProductStatusRequest request,
                CancellationToken cancellationToken)
        {
            await _productService.UpdateProductStatusAsync(
                productId,
                request,
                cancellationToken);


            return NoContent();
        }


        [HttpPut(
            "products/{productId:guid}/coverages/" +
            "{coverageId:guid}/limit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult>
            UpdateCoverageLimitAsync(
                Guid productId,
                Guid coverageId,
                [FromBody]
                    UpdateCoverageLimitRequest request,
                CancellationToken cancellationToken)
        {
            await _productService.UpdateCoverageLimitAsync(
                productId,
                coverageId,
                request,
                cancellationToken);


            return NoContent();
        }


        [HttpPut("pricing")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdatePricingAsync(
            [FromBody] UpdatePricingRequest request,
            CancellationToken cancellationToken)
        {
            await _productService.UpdatePricingAsync(
                request,
                cancellationToken);


            return NoContent();
        }


        [HttpGet("products")]
        [ProducesResponseType(
    typeof(IReadOnlyCollection<ProductDetailsDto>),
    StatusCodes.Status200OK)]
        [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
    StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<
    IReadOnlyCollection<ProductDetailsDto>>>
    GetProductsAsync(
        CancellationToken cancellationToken)
        {
            IReadOnlyCollection<ProductDetailsDto> products =
                await _productService
                    .GetAdminProductsAsync(
                        cancellationToken);


            return Ok(products);
        }


        [HttpGet("products/{productId:guid}")]
        [ProducesResponseType(
            typeof(ProductDetailsDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDetailsDto>>
            GetProductAsync(
                Guid productId,
                CancellationToken cancellationToken)
        {
            ProductDetailsDto product =
                await _productService
                    .GetAdminProductAsync(
                        productId,
                        cancellationToken);


            return Ok(product);
        }


        [HttpGet("pricing")]
        [ProducesResponseType(
            typeof(PricingConfigurationDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        public async Task<
            ActionResult<PricingConfigurationDto>>
            GetPricingAsync(
                CancellationToken cancellationToken)
        {
            PricingConfigurationDto pricing =
                await _productService.GetPricingAsync(
                    cancellationToken);


            return Ok(pricing);
        }
    }
}
