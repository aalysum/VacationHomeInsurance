using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Application.Interfaces.Services;
using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.API.Controllers
{
    [ApiController]
    [Route("claims")]
    [Authorize]
    public sealed class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;


        public ClaimsController(
            IClaimService claimService)
        {
            _claimService = claimService;
        }


        [HttpPost]
        public async Task<IActionResult> Create(
            CreateClaimRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(
                await _claimService.CreateAsync(
                    request,
                    cancellationToken));
        }

        [HttpGet("eligible-coverages")]
        public async Task<IActionResult> GetEligibleCoverages(
            [FromQuery] Guid policyId,
            [FromQuery] ClaimType incidentType,
            CancellationToken cancellationToken)
        {
            return Ok(
                await _claimService.GetEligibleCoveragesAsync(
                    policyId,
                    incidentType,
                    cancellationToken));
        }


        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] ClaimSearchRequest request)
        {
            return Ok(
                await _claimService.SearchAsync(request));
        }


        [HttpGet("{claimId:guid}")]
        public async Task<IActionResult> Get(
            Guid claimId)
        {
            return Ok(
                await _claimService.GetAsync(claimId));
        }


        [HttpPut("{claimId:guid}")]
        public async Task<IActionResult> Update(
            Guid claimId,
            UpdateClaimRequest request)
        {
            await _claimService.UpdateAsync(
                claimId,
                request);


            return NoContent();
        }


        [HttpPost("{claimId:guid}/withdraw")]
        public async Task<IActionResult> Withdraw(
            Guid claimId,
            WithdrawClaimRequest request)
        {
            await _claimService.WithdrawAsync(
                claimId,
                request);


            return NoContent();
        }
    }
}
