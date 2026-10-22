using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.PolicyService.Application.DTOs.Policies;
using VacationHome.PolicyService.Application.DTOs.Documents;
using VacationHome.PolicyService.Application.Interfaces.Services;


namespace VacationHome.PolicyService.API.Controllers
{
    [ApiController]
    [Route("admin/policies")]
    [Authorize(Roles = "Admin")]
    public sealed class AdminPoliciesController
        : ControllerBase
    {
        private readonly IPolicyService
            _policyService;

        private readonly IPolicyDocumentService
            _policyDocumentService;


        public AdminPoliciesController(
            IPolicyService policyService,
            IPolicyDocumentService policyDocumentService)
        {
            _policyService =
                policyService;

            _policyDocumentService =
                policyDocumentService;
        }


        [HttpGet]
        [ProducesResponseType(
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult>
            GetPoliciesAsync(
                [FromQuery]
                    PolicyFilterRequest request,
                CancellationToken cancellationToken)
        {
            var result =
                await _policyService
                    .GetAdminPoliciesAsync(
                        request,
                        cancellationToken);


            return Ok(result);
        }


        [HttpGet("{policyId:guid}")]
        [ProducesResponseType(
            typeof(PolicyDetailsDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        public async Task<
            ActionResult<PolicyDetailsDto>>
            GetPolicyAsync(
                Guid policyId,
                CancellationToken cancellationToken)
        {
            PolicyDetailsDto policy =
                await _policyService.GetAsync(
                    policyId,
                    Guid.Empty,
                    isAdmin: true,
                    cancellationToken);


            return Ok(policy);
        }

        [HttpGet("{policyId:guid}/documents")]
        public async Task<ActionResult<IReadOnlyList<PolicyDocumentDto>>>
            GetDocuments(
                Guid policyId,
                CancellationToken cancellationToken)
        {
            IReadOnlyList<PolicyDocumentDto> documents =
                await _policyDocumentService.GetForAdminAsync(
                    policyId,
                    cancellationToken);

            return Ok(documents);
        }

        [HttpGet("{policyId:guid}/documents/{documentId:guid}")]
        public async Task<IActionResult> DownloadDocument(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            PolicyDocumentDownload? document =
                await _policyDocumentService.DownloadForAdminAsync(
                    policyId,
                    documentId,
                    cancellationToken);

            return document is null
                ? NotFound()
                : File(
                    document.Content,
                    document.ContentType,
                    document.FileName);
        }

        [HttpPost("{policyId:guid}/documents/retry")]
        public async Task<ActionResult<PolicyDocumentDto>> RetryDocumentGeneration(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            PolicyDocumentDto document =
                await _policyDocumentService.EnsureLatestGeneratedAsync(
                    policyId,
                    cancellationToken);

            return Ok(document);
        }
    }
}
