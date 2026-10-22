using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.Auth.Application.DTOs;
using VacationHome.Auth.Application.Interfaces.Services;

namespace VacationHome.Auth.API.Controllers
{
    [ApiController]
    [Route("internal/users")]
    [Authorize(Policy = "ClaimsServiceOnly")]
    public sealed class InternalUsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public InternalUsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("{userId}/customer-contact")]
        [ProducesResponseType(typeof(InternalCustomerContactResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InternalCustomerContactResponse>>
            GetCustomerContactAsync(
                string userId,
                CancellationToken cancellationToken)
        {
            var contact =
                await _authService.GetInternalCustomerContactAsync(
                    userId,
                    cancellationToken);

            return contact is null
                ? NotFound()
                : Ok(contact);
        }
    }
}
