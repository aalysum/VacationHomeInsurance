using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.Customer.Application.DTOs.Admin;
using VacationHome.Customer.Application.Interfaces.Services;


namespace VacationHome.Customer.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("admin/customers")]
    public class AdminCustomersController : ControllerBase
    {
        private readonly IAdminCustomerService
            _adminCustomerService;


        public AdminCustomersController(
            IAdminCustomerService adminCustomerService)
        {
            _adminCustomerService =
                adminCustomerService;
        }


        /// <summary>
        /// Returns a paginated and filtered list of customers.
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
        public async Task<IActionResult> GetCustomers(
            [FromQuery] AdminCustomerQueryParameters query,
            CancellationToken cancellationToken)
        {
            var response =
                await _adminCustomerService
                    .GetCustomersAsync(
                        query,
                        cancellationToken);


            return Ok(response);
        }


        /// <summary>
        /// Returns summary details for a specific customer.
        /// </summary>
        [HttpGet("{customerId:guid}")]
        [ProducesResponseType(
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomer(
            [FromRoute] Guid customerId,
            CancellationToken cancellationToken)
        {
            var response =
                await _adminCustomerService
                    .GetCustomerAsync(
                        customerId,
                        cancellationToken);


            return Ok(response);
        }


        /// <summary>
        /// Returns a paginated list of properties belonging to
        /// the specified customer.
        /// </summary>
        [HttpGet("{customerId:guid}/properties")]
        [ProducesResponseType(
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult>
            GetCustomerProperties(
                [FromRoute] Guid customerId,
                [FromQuery]
                AdminCustomerPropertyQueryParameters query,
                CancellationToken cancellationToken)
        {
            var response =
                await _adminCustomerService
                    .GetCustomerPropertiesAsync(
                        customerId,
                        query,
                        cancellationToken);


            return Ok(response);
        }
    }
}
