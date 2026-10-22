using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.Auth.Application.DTOs;
using VacationHome.Auth.Application.Exceptions;
using VacationHome.Auth.Application.Interfaces.Services;


namespace VacationHome.Auth.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;


        private readonly ICurrentUserService
            _currentUserService;


        public AuthController(
            IAuthService authService,
            ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }


        [AllowAnonymous]
        [HttpPost("customers/register")]
        public async Task<IActionResult> RegisterCustomer(
            [FromBody] RegisterCustomerRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.RegisterCustomerAsync(
                request,
                cancellationToken);


            return Ok(
                new
                {
                    message =
                        "Customer registered successfully."
                });
        }


        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        [HttpPost("claims-managers/register")]
        public async Task<IActionResult>
            RegisterClaimsManager(
                [FromForm]
        RegisterClaimsManagerRequest request,
                IFormFile? verificationDocument,
                CancellationToken cancellationToken)
        {
            var document =
                await CreateClaimsManagerDocumentAsync(
                    verificationDocument,
                    cancellationToken);


            await _authService
                .RegisterClaimsManagerAsync(
                    request,
                    document,
                    cancellationToken);


            return Ok(
                new
                {
                    message =
                        "Claims Manager registered successfully.",
                    status =
                        "PendingApproval"
                });
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var response =
                await _authService.LoginAsync(
                    request,
                    cancellationToken);


            return Ok(response);
        }


        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var response =
                await _authService.RefreshTokenAsync(
                    request,
                    cancellationToken);


            return Ok(response);
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
            [FromBody] LogoutRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.LogoutAsync(
                request.RefreshToken,
                cancellationToken);


            return NoContent();
        }


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.ForgotPasswordAsync(
                request,
                cancellationToken);


          
            return Ok(
                new
                {
                    message =
                        "If the account exists, password-reset instructions have been sent."
                });
        }


        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.ResetPasswordAsync(
                request,
                cancellationToken);


            return Ok(
                new
                {
                    message =
                        "Password was reset successfully. Please log in again."
                });
        }


        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.ChangePasswordAsync(
                _currentUserService.UserId,
                request,
                cancellationToken);


            return NoContent();
        }


        [AllowAnonymous]
        [HttpPost(
            "claims-managers/application-status")]
        public async Task<IActionResult>
            GetClaimsManagerApplicationStatus(
                [FromBody]
                ClaimsManagerApplicationTokenRequest request,
                CancellationToken cancellationToken)
        {
            var response =
                await _authService
                    .GetClaimsManagerApplicationStatusByTokenAsync(
                        request.ApplicationToken,
                        cancellationToken);


            return Ok(response);
        }


         [AllowAnonymous]
        [Consumes("multipart/form-data")]
        [HttpPost("claims-managers/reapply")]
        public async Task<IActionResult>
            ReapplyClaimsManager(
                [FromForm]
        ReapplyClaimsManagerRequest request,
                [FromForm]
        string applicationToken,
                IFormFile? verificationDocument,
                CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(
                    applicationToken))
            {
                throw new BadRequestException(
                    "CLAIMS_MANAGER_APPLICATION_TOKEN_REQUIRED",
                    "Claims Manager application token is required.");
            }


            var document =
                await CreateClaimsManagerDocumentAsync(
                    verificationDocument,
                    cancellationToken);


            await _authService
                .ReapplyClaimsManagerAsync(
                    applicationToken,
                    request,
                    document,
                    cancellationToken);


            return Ok(
                new
                {
                    message =
                        "Claims Manager application was resubmitted successfully.",
                    status =
                        "PendingApproval"
                });
        }


        [Authorize]
        [HttpPost("mfa/setup")]
        public async Task<IActionResult> BeginMfaSetup(
            CancellationToken cancellationToken)
        {
            var response =
                await _authService.BeginMfaSetupAsync(
                    _currentUserService.UserId,
                    cancellationToken);


            return Ok(response);
        }


        [Authorize]
        [HttpPost("mfa/confirm")]
        public async Task<IActionResult> ConfirmMfaSetup(
            [FromBody]
            ConfirmMfaSetupRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.ConfirmMfaSetupAsync(
                _currentUserService.UserId,
                request,
                cancellationToken);


            return NoContent();
        }


        [AllowAnonymous]
        [HttpPost("mfa/verify")]
        public async Task<IActionResult> VerifyMfa(
            [FromBody] VerifyMfaRequest request,
            CancellationToken cancellationToken)
        {
            var response =
                await _authService.VerifyMfaAsync(
                    request,
                    cancellationToken);


            return Ok(response);
        }


        [Authorize]
        [HttpPost("mfa/disable")]
        public async Task<IActionResult> DisableMfa(
            [FromBody]
            ConfirmMfaSetupRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.DisableMfaAsync(
                _currentUserService.UserId,
                request,
                cancellationToken);


            return NoContent();
        }


        private static async Task<
            ClaimsManagerDocumentDto>
            CreateClaimsManagerDocumentAsync(
                IFormFile? verificationDocument,
                CancellationToken cancellationToken)
        {
            if (verificationDocument is null)
            {
                throw new BadRequestException(
                    "VERIFICATION_DOCUMENT_REQUIRED",
                    "Verification document is required.");
            }


            if (verificationDocument.Length == 0)
            {
                throw new BadRequestException(
                    "VERIFICATION_DOCUMENT_EMPTY",
                    "Verification document cannot be empty.");
            }


            
            const long maxFileSize =
                10 * 1024 * 1024;


            if (verificationDocument.Length >
                maxFileSize)
            {
                throw new BadRequestException(
                    "VERIFICATION_DOCUMENT_TOO_LARGE",
                    "Verification document cannot exceed 10 MB.");
            }


            await using var memoryStream =
                new MemoryStream(
                    capacity:
                        checked(
                            (int)
                            verificationDocument.Length));


            await verificationDocument.CopyToAsync(
                memoryStream,
                cancellationToken);


            return new ClaimsManagerDocumentDto
            {
                FileName =
                    Path.GetFileName(
                        verificationDocument.FileName),


                ContentType =
                    verificationDocument.ContentType,


                Content =
                    memoryStream.ToArray()
            };
        }
    }
}
