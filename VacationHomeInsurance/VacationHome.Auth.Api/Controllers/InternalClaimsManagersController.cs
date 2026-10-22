using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.Auth.Application.DTOs;
using VacationHome.Auth.Application.Interfaces.Services;


namespace VacationHome.Auth.API.Controllers
{
    [ApiController]
    [Route("internal/claims-managers")]
    [Authorize(
        Policy =
            "InternalServiceOnly")]
    public sealed class
        InternalClaimsManagersController
        : ControllerBase
    {
        private readonly IAuthService
            _authService;


        public InternalClaimsManagersController(
            IAuthService authService)
        {
            _authService =
                authService;
        }


        [HttpGet(
            "{userId}/eligibility")]
        [ProducesResponseType(
            typeof(
                ActiveClaimsManagerResponse),
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
            ActionResult<
                ActiveClaimsManagerResponse>>
            GetEligibilityAsync(
                string userId,
                CancellationToken cancellationToken)
        {
            if (
                string.IsNullOrWhiteSpace(
                    userId)
            )
            {
                return BadRequest(
                    new
                    {
                        code =
                            "CLAIMS_MANAGER_USER_ID_REQUIRED",


                        message =
                            "Claims Manager user identifier is required."
                    });
            }


            ActiveClaimsManagerResponse?
                claimsManager =
                    await _authService
                        .GetActiveClaimsManagerAsync(
                            userId.Trim(),
                            cancellationToken);


            if (claimsManager is null)
            {
                return NotFound(
                    new
                    {
                        code =
                            "CLAIMS_MANAGER_NOT_ELIGIBLE",


                        message =
                            "The selected user is not an active, approved Claims Manager."
                    });
            }


            return Ok(
                claimsManager);
        }
    }
}
