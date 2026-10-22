using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.PolicyService.Application
    .DTOs.Policies;
using VacationHome.PolicyService.Application.DTOs.Documents;
using VacationHome.PolicyService.Application
    .Interfaces.Services;


namespace VacationHome.PolicyService.API.Controllers
{
    [ApiController]
    [Route("policies")]
    [Authorize(Roles = "Customer")]
    public sealed class PoliciesController
        : ControllerBase
    {
        private readonly IPolicyService
            _policyService;


        private readonly ICustomerIdentityService
            _customerIdentityService;

        private readonly IPolicyDocumentService
            _policyDocumentService;


        public PoliciesController(
            IPolicyService policyService,
            ICustomerIdentityService customerIdentityService,
            IPolicyDocumentService policyDocumentService)
        {
            _policyService =
                policyService;


            _customerIdentityService =
                customerIdentityService;

            _policyDocumentService =
                policyDocumentService;
        }


        [HttpGet]
        public async Task<IActionResult>
            GetPolicies(
                CancellationToken cancellationToken)
        {
            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            var result =
                await _policyService
                    .GetCustomerPoliciesAsync(
                        customerId,
                        cancellationToken);


            return Ok(
                result);
        }


        [HttpGet("{policyId:guid}")]
        public async Task<IActionResult> Get(
            [FromRoute]
                Guid policyId,
            CancellationToken cancellationToken)
        {
            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            var result =
                await _policyService.GetAsync(
                    policyId,
                    customerId,
                    isAdmin: false,
                    cancellationToken);


            return Ok(
                result);
        }


        [HttpPost("{policyId:guid}/cancel")]
        public async Task<IActionResult> Cancel(
            [FromRoute]
                Guid policyId,
            [FromBody]
                CancelPolicyRequest request,
            CancellationToken cancellationToken)
        {
            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            await _policyService.CancelAsync(
                policyId,
                customerId,
                request,
                cancellationToken);


            return NoContent();
        }

        [HttpGet("{policyId:guid}/documents")]
        public async Task<ActionResult<IReadOnlyList<PolicyDocumentDto>>>
            GetDocuments(
                Guid policyId,
                CancellationToken cancellationToken)
        {
            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(cancellationToken);

            IReadOnlyList<PolicyDocumentDto> documents =
                await _policyDocumentService.GetForCustomerAsync(
                    policyId,
                    customerId,
                    cancellationToken);

            return Ok(documents);
        }

        [HttpGet("{policyId:guid}/documents/{documentId:guid}")]
        public async Task<IActionResult> DownloadDocument(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(cancellationToken);

            PolicyDocumentDownload? document =
                await _policyDocumentService.DownloadForCustomerAsync(
                    policyId,
                    documentId,
                    customerId,
                    cancellationToken);

            return document is null
                ? NotFound()
                : File(
                    document.Content,
                    document.ContentType,
                    document.FileName);
        }
    }
}
