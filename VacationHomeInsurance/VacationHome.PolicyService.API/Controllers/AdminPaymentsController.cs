using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.PolicyService.Application.Interfaces.Services;


namespace VacationHome.PolicyService.API.Controllers
{
    [ApiController]
    [Route("admin/payments")]
    [Authorize(Roles = "Admin")]
    public sealed class AdminPaymentsController
        : ControllerBase
    {
        private readonly IPaymentService _paymentService;


        public AdminPaymentsController(
            IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }


        [HttpGet("{paymentId:guid}")]
        public async Task<IActionResult> Get(
            Guid paymentId,
            CancellationToken cancellationToken)
        {
            var result =
                await _paymentService.GetPaymentAsync(
                    paymentId,
                    Guid.Empty,
                    true,
                    cancellationToken);


            return Ok(result);
        }
    }
}
