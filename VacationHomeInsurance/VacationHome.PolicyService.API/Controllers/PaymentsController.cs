using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.PolicyService.Application
    .DTOs.Payments;
using VacationHome.PolicyService.Application
    .Interfaces.Services;


namespace VacationHome.PolicyService.API.Controllers
{
    [ApiController]
    [Route("policies/payments")]
    [Authorize(Roles = "Customer")]
    public sealed class PaymentsController
        : ControllerBase
    {
        private readonly IPaymentService
            _paymentService;


        private readonly ICustomerIdentityService
            _customerIdentityService;


        public PaymentsController(
            IPaymentService paymentService,
            ICustomerIdentityService customerIdentityService)
        {
            _paymentService =
                paymentService;


            _customerIdentityService =
                customerIdentityService;
        }


        [HttpPost]
        public async Task<IActionResult> Pay(
            [FromBody]
                PaymentRequest request,
            CancellationToken cancellationToken)
        {
            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            PaymentResultDto result =
                await _paymentService
                    .ProcessPaymentAsync(
                        customerId,
                        request,
                        cancellationToken);


            return Ok(
                result);
        }


        [HttpGet("{paymentId:guid}")]
        public async Task<IActionResult> Get(
            [FromRoute]
                Guid paymentId,
            CancellationToken cancellationToken)
        {
            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            PaymentDto result =
                await _paymentService
                    .GetPaymentAsync(
                        paymentId,
                        customerId,
                        isAdmin: false,
                        cancellationToken);


            return Ok(
                result);
        }
    }
}
