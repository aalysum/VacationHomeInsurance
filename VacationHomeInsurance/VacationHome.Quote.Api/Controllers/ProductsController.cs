using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Products;
using VacationHome.ProductQuote.Application.Interfaces.Services;


namespace VacationHome.ProductQuote.API.Controllers
{
    [ApiController]
    [Route("products")]
    public sealed class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;


        public ProductsController(
            IProductService productService)
        {
            _productService = productService;
        }


        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(
            typeof(IReadOnlyCollection<ProductDetailsDto>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<
            IReadOnlyCollection<ProductDetailsDto>>>
            GetProductsAsync(
                CancellationToken cancellationToken)
        {
            IReadOnlyCollection<ProductDetailsDto> products =
                await _productService.GetProductsAsync(
                    cancellationToken);


            return Ok(products);
        }


        [AllowAnonymous]
        [HttpGet("{productId:guid}")]
        [ProducesResponseType(
            typeof(ProductDetailsDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDetailsDto>>
            GetProductAsync(
                Guid productId,
                CancellationToken cancellationToken)
        {
            ProductDetailsDto product =
                await _productService.GetProductAsync(
                    productId,
                    cancellationToken);


            return Ok(product);
        }


        [HttpPut(
            "{productId:guid}/coverages/{coverageId:guid}/charge")]
        public async Task<IActionResult>
            UpdateCoverageCharge(
                Guid productId,
                Guid coverageId,
                UpdateCoverageChargeRequest request,
                CancellationToken cancellationToken)
        {
            await _productService
                .UpdateCoverageChargeAsync(
                    productId,
                    coverageId,
                    request,
                    cancellationToken);


            return NoContent();
        }
    }
}
