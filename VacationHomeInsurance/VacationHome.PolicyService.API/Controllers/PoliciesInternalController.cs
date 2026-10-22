using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.PolicyService.Application.DTOs.Internal;
using VacationHome.PolicyService.Application.DTOs.Policies;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Application.Interfaces.Services;
using VacationHome.PolicyService.Application.Mappers;


namespace VacationHome.PolicyService.API.Controllers.Internal
{
    [ApiController]
    [Route("internal/policies")]
    [Authorize(
        Policy =
            "InternalServiceOnly")]
    public sealed class PoliciesInternalController
        : ControllerBase
    {
        private readonly IPolicyRepository
            _policyRepository;


        private readonly IPolicyService
            _policyService;

        private readonly IPolicyDocumentService
            _policyDocumentService;

        public PoliciesInternalController(
            IPolicyRepository policyRepository,
            IPolicyService policyService,
            IPolicyDocumentService policyDocumentService)
        {
            _policyRepository =
                policyRepository;


            _policyService =
                policyService;

            _policyDocumentService =
                policyDocumentService;
        }


        [HttpGet("{policyId:guid}")]
        [ProducesResponseType(
            typeof(PolicyDetailsDto),
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
            ActionResult<PolicyDetailsDto>>
            GetPolicyAsync(
                Guid policyId,
                CancellationToken cancellationToken)
        {
            if (policyId == Guid.Empty)
            {
                return BadRequest(
                    new
                    {
                        success = false,


                        code =
                            "INVALID_POLICY_ID",


                        message =
                            "Policy identifier is required."
                    });
            }


            var policy =
                await _policyRepository
                    .GetByIdAsync(
                        policyId,
                        false,
                        cancellationToken);


            if (policy is null)
            {
                return NotFound(
                    new
                    {
                        success = false,


                        code =
                            "POLICY_NOT_FOUND",


                        message =
                            "Policy was not found."
                    });
            }


            PolicyDetailsDto response =
                PolicyMapper.ToDetailsDto(
                    policy);


            return Ok(
                response);
        }

        [HttpGet("{policyId:guid}/documents/{documentId:guid}/download")]
        [Authorize(Policy = "ClaimsServiceOnly")]
        public async Task<IActionResult> DownloadPolicyDocumentAsync(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            var download =
                await _policyDocumentService
                    .DownloadForInternalServiceAsync(
                        policyId,
                        documentId,
                        cancellationToken);

            if (download is null)
            {
                return NotFound();
            }

            return File(
                download.Content,
                download.ContentType,
                download.FileName);
        }


        [HttpGet(
            "{policyId:guid}/coverage/" +
            "{coverageId:guid}")]
        [ProducesResponseType(
            typeof(PolicyCoverageDto),
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
            ActionResult<PolicyCoverageDto>>
            GetCoverageAsync(
                Guid policyId,
                Guid coverageId,
                CancellationToken cancellationToken)
        {
            if (policyId == Guid.Empty)
            {
                return BadRequest(
                    new
                    {
                        success = false,


                        code =
                            "INVALID_POLICY_ID",


                        message =
                            "Policy identifier is required."
                    });
            }


            if (coverageId == Guid.Empty)
            {
                return BadRequest(
                    new
                    {
                        success = false,


                        code =
                            "INVALID_COVERAGE_ID",


                        message =
                            "Coverage identifier is required."
                    });
            }


            var policy =
                await _policyRepository
                    .GetByIdAsync(
                        policyId,
                        false,
                        cancellationToken);


            if (policy is null)
            {
                return NotFound(
                    new
                    {
                        success = false,


                        code =
                            "POLICY_NOT_FOUND",


                        message =
                            "Policy was not found."
                    });
            }


            var coverage =
                policy.Coverages
                    .SingleOrDefault(
                        item =>
                            item.CoverageId ==
                            coverageId);


            if (coverage is null)
            {
                return NotFound(
                    new
                    {
                        success = false,


                        code =
                            "POLICY_COVERAGE_NOT_FOUND",


                        message =
                            "The selected Coverage is not included in the Policy."
                    });
            }


            var response =
                new PolicyCoverageDto(
                    coverage.CoverageId,
                    coverage.CoverageName,
                    coverage.CoverageDescription,
                    coverage.IsMandatory,
                    coverage.CoverageCharge,
                    coverage.CoverageLimitPercentage,
                    coverage.CoverageLimitAmount);


            return Ok(
                response);
        }


        [HttpPost("check-overlap")]
        [ProducesResponseType(
            typeof(PolicyOverlapResultDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<
            ActionResult<PolicyOverlapResultDto>>
            CheckOverlapAsync(
                [FromBody]
                    PolicyOverlapRequestDto request,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(
                request);


            PolicyOverlapResultDto result =
                await _policyService
                    .CheckOverlapAsync(
                        request,
                        cancellationToken);


            return Ok(
                result);
        }


        /*
         * Preserved temporarily for existing integrations.
         *
         * New multi-Coverage Claim creation retrieves the full
         * Policy snapshot from GET /internal/policies/{policyId}
         * and validates every selected Coverage independently.
         */
        [HttpGet(
            "{policyId:guid}/coverage-validation")]
        [ProducesResponseType(
            typeof(CoverageValidationResultDto),
            StatusCodes.Status200OK)]
        public async Task<
            ActionResult<CoverageValidationResultDto>>
            ValidateCoverageAsync(
                Guid policyId,
                [FromQuery] DateTime incidentDate,
                CancellationToken cancellationToken)
        {
            CoverageValidationResultDto result =
                await _policyService
                    .ValidateClaimCoverageAsync(
                        policyId,
                        incidentDate,
                        cancellationToken);


            return Ok(
                result);
        }


        /*
         * Preserved temporarily for backwards compatibility.
         *
         * The new Claim workflow must not use this endpoint
         * because it sums all Policy Coverage limits. Each
         * selected Coverage is now validated independently.
         */
        [HttpGet(
            "{policyId:guid}/claim-amount-validation")]
        [ProducesResponseType(
            typeof(CoverageValidationResultDto),
            StatusCodes.Status200OK)]
        public async Task<
            ActionResult<CoverageValidationResultDto>>
            ValidateClaimAmountAsync(
                Guid policyId,
                [FromQuery] decimal claimAmount,
                CancellationToken cancellationToken)
        {
            CoverageValidationResultDto result =
                await _policyService
                    .ValidateClaimAmountAsync(
                        policyId,
                        claimAmount,
                        cancellationToken);


            return Ok(
                result);
        }
    }
}
