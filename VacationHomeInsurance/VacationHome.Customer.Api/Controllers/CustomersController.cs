using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.Customer.Application.DTOs.Customers;
using VacationHome.Customer.Application.Interfaces.Services;


namespace VacationHome.Customer.API.Controllers
{
    [ApiController]
    [Route("customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService
            _customerService;


        private readonly ICustomerOnboardingService
            _customerOnboardingService;


        public CustomersController(
            ICustomerService customerService,
            ICustomerOnboardingService customerOnboardingService)
        {
            _customerService =
                customerService;


            _customerOnboardingService =
                customerOnboardingService;
        }


        [Authorize(Roles = "Customer")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(
            CancellationToken cancellationToken)
        {
            var response =
                await _customerService.GetProfileAsync(
                    cancellationToken);


            return Ok(response);
        }


        [Authorize(Roles = "Customer")]
        [HttpPatch("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateCustomerProfileRequest request,
            CancellationToken cancellationToken)
        {
            await _customerService.UpdateProfileAsync(
                request,
                cancellationToken);


            return NoContent();
        }




        [Authorize(Policy = "AuthServiceOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(
            [FromBody] CreateCustomerRequest request,
            CancellationToken cancellationToken)
        {
            var response =
                await _customerOnboardingService.CreateCustomerAsync(
                    request,
                    cancellationToken);


            if (response.AlreadyExisted)
            {
                return Ok(response);
            }


            return StatusCode(
                StatusCodes.Status201Created,
                response);
        }


        [Authorize(Policy = "AuthServiceOnly")]
        [HttpPut("{authUserId}/status")]
        public async Task<IActionResult> UpdateCustomerStatus(
            [FromRoute] string authUserId,
            [FromBody] UpdateCustomerStatusRequest request,
            CancellationToken cancellationToken)
        {
            await _customerOnboardingService.UpdateCustomerStatusAsync(
                authUserId,
                request,
                cancellationToken);


            return NoContent();
        }


    }
}
