using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.Customer.Application.Interfaces.Services;


namespace VacationHome.Customer.API.Controllers
{
    [ApiController]
    [Authorize(Policy = "ProductQuoteServiceOnly")]
    [Route("internal/properties")]
    public class InternalPropertiesController : ControllerBase
    {
        private readonly IInternalPropertyService
            _internalPropertyService;


        public InternalPropertiesController(
            IInternalPropertyService internalPropertyService)
        {
            _internalPropertyService =
                internalPropertyService;
        }


        [HttpGet("{propertyId:guid}")]
        public async Task<IActionResult> GetPropertySnapshot(
    [FromRoute] Guid propertyId,
    CancellationToken cancellationToken)
        {
            var response =
                await _internalPropertyService
                    .GetPropertySnapshotAsync(
                        propertyId,
                        cancellationToken);


            return Ok(response);
        }
    }
}
