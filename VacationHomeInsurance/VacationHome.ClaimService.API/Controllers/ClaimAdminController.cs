using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.ClaimService.Application.Common;
using VacationHome.ClaimService.Application.DTOs.Admin;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Application.Interfaces.Services;


namespace VacationHome.ClaimService.API.Controllers
{
    [ApiController]
    [Route("admin/claims")]
    [Authorize(
        Roles =
            "Admin,ClaimsManager")]
    public sealed class ClaimAdminController
        : ControllerBase
    {
        private readonly IClaimAdminService
            _claimAdminService;


        private readonly IClaimWorkflowService
            _workflowService;


        public ClaimAdminController(
            IClaimAdminService claimAdminService,
            IClaimWorkflowService workflowService)
        {
            _claimAdminService =
                claimAdminService;


            _workflowService =
                workflowService;
        }


        /*
         * Admin and Claims Manager can view dashboard data.
         */
        [HttpGet("dashboard")]
        [ProducesResponseType(
            typeof(ClaimDashboardDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<
            ActionResult<ClaimDashboardDto>>
            DashboardAsync(
                CancellationToken cancellationToken)
        {
            ClaimDashboardDto result =
                await _claimAdminService
                    .GetDashboardAsync(
                        cancellationToken);


            return Ok(result);
        }


        /*
         * Admin and Claims Manager can search Claims.
         *
         * Later, the Claims Manager query must be restricted
         * to Claims assigned to the authenticated manager.
         */
        [HttpGet]
        [ProducesResponseType(
            typeof(PagedResult<ClaimDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<
            ActionResult<PagedResult<ClaimDto>>>
            SearchAsync(
                [FromQuery]
                    ClaimDashboardFilterRequest request,
                CancellationToken cancellationToken)
        {
            PagedResult<ClaimDto> result =
                await _claimAdminService
                    .GetClaimsAsync(
                        request,
                        cancellationToken);


            return Ok(result);
        }


        /*
         * Admin and Claims Manager can view Claim details.
         *
         * Claims Manager access must eventually verify that
         * the Claim is assigned to that manager.
         */
        [HttpGet("{claimId:guid}")]
        [ProducesResponseType(
            typeof(ClaimDetailDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        public async Task<
            ActionResult<ClaimDetailDto>>
            GetAsync(
                Guid claimId,
                CancellationToken cancellationToken)
        {
            ClaimDetailDto result =
                await _claimAdminService
                    .GetClaimAsync(
                        claimId,
                        cancellationToken);


            return Ok(result);
        }

        [HttpGet(
            "{claimId:guid}/policy-documents/{documentId:guid}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadPolicyDocumentAsync(
            Guid claimId,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            var download =
                await _claimAdminService.DownloadPolicyDocumentAsync(
                    claimId,
                    documentId,
                    cancellationToken);

            return File(
                download.Content,
                download.ContentType,
                download.FileName);
        }


        /*
         * Only Admin can assign or reassign a Claims Manager.
         *
         * This action does not change the Claim status and
         * does not make a Claim decision.
         */
        [HttpPost(
            "{claimId:guid}/assignment")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(
            typeof(ClaimDetailDto),
            StatusCodes.Status200OK)]
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
        public async Task<
            ActionResult<ClaimDetailDto>>
            AssignClaimsManagerAsync(
                Guid claimId,
                [FromBody]
                    AssignClaimsManagerRequest request,
                CancellationToken cancellationToken)
        {
            ClaimDetailDto result =
                await _claimAdminService
                    .AssignClaimsManagerAsync(
                        claimId,
                        request,
                        cancellationToken);


            return Ok(result);
        }


        /*
         * Only the Claims Manager role can perform Claim
         * workflow actions. The workflow service must also
         * verify that the Claim is assigned to the current
         * Claims Manager.
         */
        [HttpPost(
            "{claimId:guid}/review")]
        [Authorize(Roles = "ClaimsManager")]
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
        public async Task<IActionResult>
            ReviewAsync(
                Guid claimId,
                [FromBody]
                    ClaimRowVersionRequest request,
                CancellationToken cancellationToken)
        {
            await _workflowService
                .MoveToReviewAsync(
                    claimId,
                    request.RowVersion,
                    cancellationToken);


            return NoContent();
        }


        [HttpPost(
            "{claimId:guid}/request-information")]
        [Authorize(Roles = "ClaimsManager")]
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
        public async Task<IActionResult>
            RequestInformationAsync(
                Guid claimId,
                [FromBody]
                    RequestInformationRequest request,
                CancellationToken cancellationToken)
        {
            await _workflowService
                .RequestInformationAsync(
                    claimId,
                    request,
                    cancellationToken);


            return NoContent();
        }


        [HttpPost(
            "{claimId:guid}/approve")]
        [Authorize(Roles = "ClaimsManager")]
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
        public async Task<IActionResult>
            ApproveAsync(
                Guid claimId,
                [FromBody]
                    ApproveClaimRequest request,
                CancellationToken cancellationToken)
        {
            await _workflowService
                .ApproveAsync(
                    claimId,
                    request,
                    cancellationToken);


            return NoContent();
        }


        [HttpPost(
            "{claimId:guid}/reject")]
        [Authorize(Roles = "ClaimsManager")]
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
        public async Task<IActionResult>
            RejectAsync(
                Guid claimId,
                [FromBody]
                    RejectClaimRequest request,
                CancellationToken cancellationToken)
        {
            await _workflowService
                .RejectAsync(
                    claimId,
                    request,
                    cancellationToken);


            return NoContent();
        }


        [HttpPost(
            "{claimId:guid}/pay")]
        [Authorize(Roles = "ClaimsManager")]
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
        public async Task<IActionResult>
            PayAsync(
                Guid claimId,
                [FromBody]
                    MarkClaimPaidRequest request,
                CancellationToken cancellationToken)
        {
            await _workflowService
                .MarkPaidAsync(
                    claimId,
                    request,
                    cancellationToken);


            return NoContent();
        }


        [HttpPost(
            "{claimId:guid}/close")]
        [Authorize(Roles = "ClaimsManager")]
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
        public async Task<IActionResult>
            CloseAsync(
                Guid claimId,
                [FromBody]
                    ClaimRowVersionRequest request,
                CancellationToken cancellationToken)
        {
            await _workflowService
                .CloseAsync(
                    claimId,
                    request.RowVersion,
                    cancellationToken);


            return NoContent();
        }
    }
}
