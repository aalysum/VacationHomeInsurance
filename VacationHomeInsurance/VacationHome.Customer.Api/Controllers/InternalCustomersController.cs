using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.Customer.Application.Interfaces.Services;


namespace VacationHome.Customer.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "InternalServiceOnly")]
    [Route("internal/customers")]
    public class InternalCustomersController : ControllerBase
    {
        private readonly IInternalCustomerService
            _internalCustomerService;


        public InternalCustomersController(
            IInternalCustomerService internalCustomerService)
        {
            _internalCustomerService =
                internalCustomerService;
        }


        [HttpGet("{customerId:guid}/status")]
        public async Task<IActionResult> GetStatus(
            Guid customerId,
            CancellationToken cancellationToken)
        {
            var response =
                await _internalCustomerService
                    .GetCustomerStatusAsync(
                        customerId,
                        cancellationToken);


            return Ok(response);
        }


        [HttpGet(
            "by-auth-user/{authUserId}/status")]
        public async Task<IActionResult>
            GetStatusByAuthUserId(
                string authUserId,
                CancellationToken cancellationToken)
        {
            var response =
                await _internalCustomerService
                    .GetCustomerStatusByAuthUserIdAsync(
                        authUserId,
                        cancellationToken);


            return Ok(response);
        }


        [HttpGet("{customerId:guid}")]
        public async Task<IActionResult> GetCustomer(
            Guid customerId,
            CancellationToken cancellationToken)
        {
            var response =
                await _internalCustomerService
                    .GetCustomerAsync(
                        customerId,
                        cancellationToken);


            return Ok(response);
        }


    }


}
