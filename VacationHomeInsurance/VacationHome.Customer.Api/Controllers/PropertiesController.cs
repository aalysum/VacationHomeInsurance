using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.Customer.Application.DTOs.Properties;
using VacationHome.Customer.Application.Interfaces.Services;


namespace VacationHome.Customer.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Customer")]
    [Route("properties")]
    public class PropertiesController : ControllerBase
    {
        private readonly ICustomerService _customerService;


        public PropertiesController(
            ICustomerService customerService)
        {
            _customerService = customerService;
        }


        /// <summary>
        /// Creates a vacation property for the authenticated customer.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateProperty(
            [FromBody] CreateVacationPropertyRequest request,
            CancellationToken cancellationToken)
        {
            var propertyId =
                await _customerService.CreatePropertyAsync(
                    request,
                    cancellationToken);


            return CreatedAtAction(
                nameof(GetProperty),
                new
                {
                    id = propertyId
                },
                new
                {
                    propertyId
                });
        }


        /// <summary>
        /// Returns a paginated list of properties owned by the
        /// authenticated customer.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetProperties(
            [FromQuery] PropertyQueryParameters query,
            CancellationToken cancellationToken)
        {
            var response =
                await _customerService.GetPropertiesAsync(
                    query,
                    cancellationToken);


            return Ok(response);
        }


        /// <summary>
        /// Returns a property owned by the authenticated customer.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProperty(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var response =
                await _customerService.GetPropertyAsync(
                    id,
                    cancellationToken);


            return Ok(response);
        }


        /// <summary>
        /// Updates a property owned by the authenticated customer.
        /// The request must contain the latest row version.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(
            StatusCodes.Status204NoContent)]
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
        public async Task<IActionResult> UpdateProperty(
            [FromRoute] Guid id,
            [FromBody] UpdateVacationPropertyRequest request,
            CancellationToken cancellationToken)
        {
            await _customerService.UpdatePropertyAsync(
                id,
                request,
                cancellationToken);


            return NoContent();
        }


        /// <summary>
        /// Archives a property owned by the authenticated customer.
        /// Archiving does not physically delete the property.
        /// </summary>
        [HttpPatch("{id:guid}/archive")]
        [ProducesResponseType(
            StatusCodes.Status204NoContent)]
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
        public async Task<IActionResult> ArchiveProperty(
            [FromRoute] Guid id,
            [FromBody] ArchiveVacationPropertyRequest request,
            CancellationToken cancellationToken)
        {
            await _customerService.ArchivePropertyAsync(
                id,
                request,
                cancellationToken);


            return NoContent();
        }
    }
}
