using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.Auth.Application.DTOs;
using VacationHome.Auth.Application.Interfaces.Services;
using VacationHome.Auth.Domain.Constants;


namespace VacationHome.Auth.API.Controllers
{
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    [Route("admin")]
    public sealed class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;


        private readonly ICurrentUserService
            _currentUserService;


        public AdminController(
            IAuthService authService,
            ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }


        [HttpPut(
            "claims-managers/{id:guid}/approve")]
        public async Task<IActionResult>
            ApproveClaimsManager(
                Guid id,
                CancellationToken cancellationToken)
        {
            await _authService
                .ApproveClaimsManagerAsync(
                    id,
                    _currentUserService.UserId,
                    cancellationToken);


            return NoContent();
        }


        [HttpPut(
            "claims-managers/{id:guid}/reject")]
        public async Task<IActionResult>
            RejectClaimsManager(
                Guid id,
                [FromBody]
                RejectClaimsManagerRequest request,
                CancellationToken cancellationToken)
        {
            await _authService
                .RejectClaimsManagerAsync(
                    id,
                    request,
                    _currentUserService.UserId,
                    cancellationToken);


            return NoContent();
        }


        [HttpPut("users/{id}/status")]
        public async Task<IActionResult>
            UpdateUserStatus(
                string id,
                [FromBody]
                UpdateUserStatusRequest request,
                CancellationToken cancellationToken)
        {
            await _authService
                .UpdateUserStatusAsync(
                    id,
                    request,
                    _currentUserService.UserId,
                    cancellationToken);


            return NoContent();
        }


        [HttpGet("claims-managers/pending")]
        public async Task<IActionResult>
            GetPendingClaimsManagers(
                CancellationToken cancellationToken)
        {
            var result =
                await _authService
                    .GetPendingClaimsManagersAsync(
                        cancellationToken);


            return Ok(result);
        }


        [HttpGet(
            "claims-managers/{id:guid}/document")]
        public async Task<IActionResult>
            GetVerificationDocument(
                Guid id,
                CancellationToken cancellationToken)
        {
            var file =
                await _authService
                    .GetVerificationDocumentAsync(
                        id,
                        _currentUserService.UserId,
                        cancellationToken);


            return File(
                file.Content,
                file.ContentType,
                file.DownloadFileName);
        }


        [HttpGet("claims-managers/active")]
        [ProducesResponseType(
            typeof(
                IReadOnlyCollection<
                    ActiveClaimsManagerResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<
            IReadOnlyCollection<
                ActiveClaimsManagerResponse>>>
            GetActiveClaimsManagers(
                CancellationToken cancellationToken)
        {
            IReadOnlyCollection<
                ActiveClaimsManagerResponse>
                claimsManagers =
                    await _authService
                        .GetActiveClaimsManagersAsync(
                            cancellationToken);


            return Ok(claimsManagers);
        }


    }
}
