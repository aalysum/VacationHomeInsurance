using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.PolicyService.Application.Interfaces.Services;


namespace VacationHome.PolicyService.API.Controllers
{
    [ApiController]
    [Route("policies")]
    [Authorize(Roles = "Customer")]
    public sealed class RenewalsController : ControllerBase
    {
        private readonly IRenewalService
            _renewalService;


        private readonly ICustomerIdentityService
            _customerIdentityService;


        public RenewalsController(
            IRenewalService renewalService,
            ICustomerIdentityService customerIdentityService)
        {
            _renewalService =
                renewalService;


            _customerIdentityService =
                customerIdentityService;
        }


        [HttpGet("{policyId:guid}/renewal-eligibility")]
        public async Task<IActionResult> Eligibility(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            var result =
                await _renewalService
                    .CheckEligibilityAsync(
                        policyId,
                        customerId,
                        cancellationToken);


            return Ok(result);
        }


        [HttpPost("{policyId:guid}/renew")]
        public async Task<IActionResult> Renew(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            var result =
                await _renewalService
                    .CreateRenewalAsync(
                        policyId,
                        customerId,
                        cancellationToken);


            return Ok(result);
        }
    }
}
