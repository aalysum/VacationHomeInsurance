using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Encodings.Web;
using VacationHome.Auth.Application.Common;
using VacationHome.Auth.Application.DTOs;
using VacationHome.Auth.Application.Exceptions;
using VacationHome.Auth.Application.Interfaces.Persistence;
using VacationHome.Auth.Application.Interfaces.Repositories;
using VacationHome.Auth.Application.Interfaces.Services;
using VacationHome.Auth.Domain.Constants;
using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Domain.Enums;
using VacationHome.Auth.Application.Interfaces.Clients;




namespace VacationHome.Auth.Application.Services
{
    public sealed class AuthService : IAuthService
    {
        private const string AuthenticatorIssuer =
            "VacationHome";


        private readonly UserManager<ApplicationUser>
            _userManager;


        private readonly IJwtTokenService
            _jwtTokenService;


        private readonly IClaimsManagerApplicationRepository
            _claimsManagerRepository;


        private readonly IRefreshTokenRepository
            _refreshTokenRepository;


        private readonly IPasswordResetTokenRepository
            _passwordResetTokenRepository;


        private readonly ILoginAttemptRepository
            _loginAttemptRepository;


        private readonly IAuditLogRepository
            _auditLogRepository;


        private readonly IFileStorageService
            _fileStorageService;


        private readonly ICustomerServiceClient
            _customerServiceClient;


        private readonly IPasswordResetNotificationService
            _passwordResetNotificationService;


        private readonly JwtSettings
            _jwtSettings;


        private readonly IAuthUnitOfWork _authUnitOfWork;


        private readonly ICurrentUserService
            _currentUserService;


        private readonly ILogger<AuthService> _logger;


        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IClaimsManagerApplicationRepository
                claimsManagerRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordResetTokenRepository
                passwordResetTokenRepository,
            ILoginAttemptRepository loginAttemptRepository,
            IAuditLogRepository auditLogRepository,
            IFileStorageService fileStorageService,
            ICustomerServiceClient customerServiceClient,
            IPasswordResetNotificationService
                passwordResetNotificationService,
            IOptions<JwtSettings> jwtOptions,
           IAuthUnitOfWork authUnitOfWork,
           ICurrentUserService currentUserService,
           ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _claimsManagerRepository =
                claimsManagerRepository;
            _refreshTokenRepository =
                refreshTokenRepository;
            _passwordResetTokenRepository =
                passwordResetTokenRepository;
            _loginAttemptRepository =
                loginAttemptRepository;
            _auditLogRepository =
                auditLogRepository;
            _fileStorageService =
                fileStorageService;
            _customerServiceClient =
                customerServiceClient;
            _passwordResetNotificationService =
                passwordResetNotificationService;
            _jwtSettings = jwtOptions.Value;
            _authUnitOfWork = authUnitOfWork;


            _currentUserService =
            currentUserService;


            _logger =
            logger;
        }


        private async Task SynchronizeCustomerStatusAsync(
            string authUserId,
            string status,
            string reason,
            CancellationToken cancellationToken)
        {
            try
            {
                await _customerServiceClient
                    .SynchronizeCustomerStatusAsync(
                        authUserId:
                            authUserId,
                        status:
                            status,
                        reason:
                            reason,
                        correlationId:
                            _currentUserService
                                .CorrelationId,
                        cancellationToken:
                            cancellationToken);
            }
            catch (Exception exception)
            {
                
                _logger.LogError(
                    exception,
                    "Customer status synchronization failed for AuthUserId {AuthUserId}, Status {Status}. CorrelationId: {CorrelationId}",
                    authUserId,
                    status,
                    _currentUserService.CorrelationId);
            }
        }


        public async Task RegisterCustomerAsync(
            RegisterCustomerRequest request,
            CancellationToken cancellationToken)
        {
            var email =
                InputValidationHelper.NormalizeEmail(
                    request.Email);


            var existingUser =
                await _userManager.FindByEmailAsync(
                    email);


            if (existingUser is not null)
            {
                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    email,
                    "CustomerRegistrationFailed",
                    null,
                    null,
                    "Email already exists.",
                    cancellationToken);


                throw new BadRequestException(
                    "EMAIL_ALREADY_EXISTS",
                    "Email already exists.");
            }


            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName =
                    InputValidationHelper.NormalizeText(
                        request.FirstName),
                LastName =
                    InputValidationHelper.NormalizeText(
                        request.LastName),
                PhoneNumber =
                    InputValidationHelper.NormalizeText(
                        request.PhoneNumber),
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };


            var createResult =
                await _userManager.CreateAsync(
                    user,
                    request.Password);


            if (!createResult.Succeeded)
            {
                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    email,
                    "CustomerRegistrationFailed",
                    null,
                    null,
                    JoinIdentityErrorCodes(
                        createResult),
                    cancellationToken);


                throw CreateIdentityBadRequest(
                    "CUSTOMER_CREATION_FAILED",
                    createResult);
            }


            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    Roles.Customer);


            if (!roleResult.Succeeded)
            {
                var cleanupResult =
                    await _userManager.DeleteAsync(user);


                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    user.Id,
                    "CustomerRegistrationFailed",
                    user.Id,
                    null,
                    cleanupResult.Succeeded
                        ? "Role assignment failed. Auth user removed."
                        : "Role assignment failed. Auth user cleanup failed.",
                    cancellationToken);


                throw new BadRequestException(
                    "CUSTOMER_ROLE_ASSIGNMENT_FAILED",
                    "Customer registration could not be completed.");
            }


            try
            {
                await _customerServiceClient.CreateCustomerAsync(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber!,
                    string.IsNullOrWhiteSpace(_currentUserService.CorrelationId)
                        ? Guid.NewGuid().ToString("N")
                        : _currentUserService.CorrelationId.Trim(),
                    cancellationToken);
            }
            catch
            {
                var cleanupResult =
                    await _userManager.DeleteAsync(user);


                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    user.Id,
                    "CustomerRegistrationFailed",
                    user.Id,
                    null,
                    cleanupResult.Succeeded
                        ? "Customer Service onboarding failed. Auth user removed."
                        : "Customer Service onboarding failed. Auth user cleanup failed.",
                    cancellationToken);


                throw new BadRequestException(
                    "CUSTOMER_ONBOARDING_FAILED",
                    "Customer registration could not be completed.");
            }


            await AddAuditLogAsync(
                nameof(ApplicationUser),
                user.Id,
                "CustomerRegistered",
                user.Id,
                null,
                null,
                cancellationToken);
        }


        public async Task RegisterClaimsManagerAsync(
            RegisterClaimsManagerRequest request,
            ClaimsManagerDocumentDto document,
            CancellationToken cancellationToken)
        {
            var email =
                InputValidationHelper.NormalizeEmail(
                    request.Email);


            var existingUser =
                await _userManager.FindByEmailAsync(email);


            if (existingUser is not null)
            {
                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    email,
                    "ClaimsManagerRegistrationFailed",
                    null,
                    null,
                    "Email already exists.",
                    cancellationToken);


                throw new BadRequestException(
                    "EMAIL_ALREADY_EXISTS",
                    "Email already exists.");
            }


            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName =
                    InputValidationHelper.NormalizeText(
                        request.FirstName),
                LastName =
                    InputValidationHelper.NormalizeText(
                        request.LastName),
                PhoneNumber =
                    InputValidationHelper.NormalizeText(
                        request.PhoneNumber),
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };


            var createResult =
                await _userManager.CreateAsync(
                    user,
                    request.Password);


            if (!createResult.Succeeded)
            {
                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    email,
                    "ClaimsManagerRegistrationFailed",
                    null,
                    null,
                    JoinIdentityErrorCodes(createResult),
                    cancellationToken);


                throw CreateIdentityBadRequest(
                    "CLAIMS_MANAGER_USER_CREATION_FAILED",
                    createResult);
            }


            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    Roles.ClaimsManager);


            if (!roleResult.Succeeded)
            {
                var cleanupResult =
                    await _userManager.DeleteAsync(user);


                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    user.Id,
                    "ClaimsManagerRegistrationFailed",
                    user.Id,
                    null,
                    cleanupResult.Succeeded
                        ? "Role assignment failed. Auth user removed."
                        : "Role assignment failed. Auth user cleanup failed.",
                    cancellationToken);


                throw new BadRequestException(
                    "CLAIMS_MANAGER_ROLE_ASSIGNMENT_FAILED",
                    "Claims Manager registration could not be completed.");
            }


            string documentPath;


            try
            {
                documentPath =
                    await _fileStorageService
                        .SaveClaimsManagerDocumentAsync(
                            document,
                            cancellationToken);
            }
            catch (BadRequestException)
            {
                var cleanupResult =
                    await _userManager.DeleteAsync(user);


                await AddAuditLogAsync(
                    nameof(ClaimsManagerApplication),
                    user.Id,
                    "ClaimsManagerRegistrationFailed",
                    user.Id,
                    null,
                    cleanupResult.Succeeded
                        ? "Document validation failed. Auth user removed."
                        : "Document validation failed. Auth user cleanup failed.",
                    cancellationToken);


                throw;
            }
            catch
            {
                var cleanupResult =
                    await _userManager.DeleteAsync(user);


                await AddAuditLogAsync(
                    nameof(ClaimsManagerApplication),
                    user.Id,
                    "ClaimsManagerRegistrationFailed",
                    user.Id,
                    null,
                    cleanupResult.Succeeded
                        ? "Document storage failed. Auth user removed."
                        : "Document storage failed. Auth user cleanup failed.",
                    cancellationToken);


                throw new BadRequestException(
                    "CLAIMS_MANAGER_DOCUMENT_STORAGE_FAILED",
                    "Claims Manager registration could not be completed.");
            }


            var application =
                new ClaimsManagerApplication
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    YearsOfExperience =
                        request.YearsOfExperience,
                    ProfessionalSummary =
                        InputValidationHelper.NormalizeText(
                            request.ProfessionalSummary),
                    VerificationDocumentPath =
                        documentPath,
                    Status =
                        ClaimsManagerApprovalStatus.PendingApproval,
                    CreatedAtUtc =
                        DateTime.UtcNow
                };


            try
            {
                await _authUnitOfWork.ExecuteInTransactionAsync(
                    async transactionToken =>
                    {
                        await _claimsManagerRepository.AddAsync(
                            application,
                            transactionToken);


                        await AddAuditLogWithoutSaveAsync(
                            nameof(ClaimsManagerApplication),
                            application.Id.ToString(),
                            "ClaimsManagerRegistered",
                            user.Id,
                            null,
                            ClaimsManagerApprovalStatus
                                .PendingApproval
                                .ToString(),
                            transactionToken);


                        await AddAuditLogWithoutSaveAsync(
                            "VerificationDocument",
                            application.Id.ToString(),
                            "DocumentUploaded",
                            user.Id,
                            null,
                            null,
                            transactionToken);
                    },
                    cancellationToken);
            }
            catch
            {
                await TryDeleteDocumentAsync(
                    documentPath,
                    cancellationToken);


                var cleanupResult =
                    await _userManager.DeleteAsync(user);


                await AddAuditLogAsync(
                    nameof(ClaimsManagerApplication),
                    application.Id.ToString(),
                    "ClaimsManagerRegistrationFailed",
                    user.Id,
                    null,
                    cleanupResult.Succeeded
                        ? "Application persistence failed. File and Auth user removed."
                        : "Application persistence failed. Auth user cleanup failed.",
                    cancellationToken);


                throw new BadRequestException(
                    "CLAIMS_MANAGER_APPLICATION_CREATION_FAILED",
                    "Claims Manager registration could not be completed.");
            }
        }


        public async Task ReapplyClaimsManagerAsync(
           string applicationToken,
           ReapplyClaimsManagerRequest request,
           ClaimsManagerDocumentDto document,
           CancellationToken cancellationToken)
        {
            var userId =
                _jwtTokenService
                    .ValidateClaimsManagerApplicationToken(
                        applicationToken);


            var user =
                await _userManager.FindByIdAsync(userId);


            if (user is null ||
                user.Status != UserStatus.Active)
            {
                throw InvalidClaimsManagerApplicationToken();
            }


            var isClaimsManager =
                await _userManager.IsInRoleAsync(
                    user,
                    Roles.ClaimsManager);


            if (!isClaimsManager)
            {
                throw new ForbiddenException(
                    "CLAIMS_MANAGER_ROLE_REQUIRED",
                    "The account is not a Claims Manager account.");
            }


            var application =
                await _claimsManagerRepository
                    .GetByUserIdAsync(
                        user.Id,
                        cancellationToken);


            if (application is null)
            {
                throw new NotFoundException(
                    "CLAIMS_MANAGER_APPLICATION_NOT_FOUND",
                    "Claims Manager application was not found.");
            }


            if (application.Status !=
                ClaimsManagerApprovalStatus.Rejected)
            {
                throw new BadRequestException(
                    "CLAIMS_MANAGER_REAPPLICATION_NOT_ALLOWED",
                    "Only a rejected Claims Manager application can be resubmitted.");
            }


            var firstName =
                InputValidationHelper.NormalizeText(
                    request.FirstName);


            var lastName =
                InputValidationHelper.NormalizeText(
                    request.LastName);


            var phoneNumber =
                InputValidationHelper.NormalizeText(
                    request.PhoneNumber);


            var summary =
                InputValidationHelper.NormalizeText(
                    request.ProfessionalSummary);


            var oldDocumentPath =
                application.VerificationDocumentPath;


            string newDocumentPath;


            try
            {
                newDocumentPath =
                    await _fileStorageService
                        .SaveClaimsManagerDocumentAsync(
                            document,
                            cancellationToken);
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch
            {
                throw new BadRequestException(
                    "CLAIMS_MANAGER_DOCUMENT_STORAGE_FAILED",
                    "Claims Manager reapplication could not be completed.");
            }


            try
            {
                await _authUnitOfWork.ExecuteInTransactionAsync(
                    async transactionToken =>
                    {
                        user.FirstName = firstName;
                        user.LastName = lastName;
                        user.PhoneNumber = phoneNumber;
                        user.UpdatedAtUtc = DateTime.UtcNow;


                        var userResult =
                            await _userManager.UpdateAsync(user);


                        if (!userResult.Succeeded)
                        {
                            throw CreateIdentityBadRequest(
                                "CLAIMS_MANAGER_PROFILE_UPDATE_FAILED",
                                userResult);
                        }


                        application.YearsOfExperience =
                            request.YearsOfExperience;


                        application.ProfessionalSummary =
                            summary;


                        application.VerificationDocumentPath =
                            newDocumentPath;


                        application.Status =
                            ClaimsManagerApprovalStatus
                                .PendingApproval;


                        application.RejectionReason = null;
                        application.ApprovedAtUtc = null;
                        application.ApprovedByUserId = null;
                        application.CreatedAtUtc =
                            DateTime.UtcNow;


                        await _claimsManagerRepository
                            .UpdateAsync(
                                application,
                                transactionToken);


                        await AddAuditLogWithoutSaveAsync(
                            nameof(ClaimsManagerApplication),
                            application.Id.ToString(),
                            "ClaimsManagerReapplied",
                            user.Id,
                            ClaimsManagerApprovalStatus
                                .Rejected
                                .ToString(),
                            ClaimsManagerApprovalStatus
                                .PendingApproval
                                .ToString(),
                            transactionToken);


                        await AddAuditLogWithoutSaveAsync(
                            "VerificationDocument",
                            application.Id.ToString(),
                            "DocumentReplaced",
                            user.Id,
                            null,
                            null,
                            transactionToken);
                    },
                    cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await TryDeleteDocumentAsync(
                    newDocumentPath,
                    cancellationToken);


                throw new BadRequestException(
                    "CLAIMS_MANAGER_REAPPLICATION_CONFLICT",
                    "The Claims Manager application was modified by another request.");
            }
            catch
            {
                await TryDeleteDocumentAsync(
                    newDocumentPath,
                    cancellationToken);


                throw;
            }


            if (!string.IsNullOrWhiteSpace(
                    oldDocumentPath) &&
                !string.Equals(
                    oldDocumentPath,
                    newDocumentPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                await TryDeleteDocumentAsync(
                    oldDocumentPath,
                    cancellationToken);
            }
        }
        public async Task<LoginResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var email =
                InputValidationHelper.NormalizeEmail(
                    request.Email);


            var user =
                await _userManager.FindByEmailAsync(email);


            if (user is null)
            {
                await RecordLoginAttemptAsync(
                    null,
                    email,
                    false,
                    cancellationToken);


                throw InvalidCredentials();
            }


            if (user.Status == UserStatus.Inactive)
            {
                await RecordLoginAttemptAsync(
                    user.Id,
                    email,
                    false,
                    cancellationToken);


                throw new ForbiddenException(
                    "ACCOUNT_INACTIVE",
                    "Account is inactive.");
            }


            if (await _userManager.IsLockedOutAsync(user))
            {
                await RecordLoginAttemptAsync(
                    user.Id,
                    email,
                    false,
                    cancellationToken);


                throw new ForbiddenException(
                    "ACCOUNT_LOCKED",
                    "Account is locked. Try again later.");
            }


            var passwordValid =
                await _userManager.CheckPasswordAsync(
                    user,
                    request.Password);


            if (!passwordValid)
            {
                await RecordLoginAttemptAsync(
                    user.Id,
                    email,
                    false,
                    cancellationToken);


                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    user.Id,
                    "LoginFailure",
                    user.Id,
                    null,
                    null,
                    cancellationToken);


                var failedResult =
                    await _userManager.AccessFailedAsync(user);


                if (!failedResult.Succeeded)
                {
                    throw new BadRequestException(
                        "LOGIN_FAILURE_UPDATE_FAILED",
                        "The login attempt could not be processed.");
                }


                if (await _userManager.IsLockedOutAsync(user))
                {
                    await AddAuditLogAsync(
                        nameof(ApplicationUser),
                        user.Id,
                        "AccountLocked",
                        user.Id,
                        null,
                        null,
                        cancellationToken);
                }


                throw InvalidCredentials();
            }


            var roles =
                await _userManager.GetRolesAsync(user);


            if (roles.Contains(Roles.ClaimsManager))
            {
                var application =
                    await _claimsManagerRepository
                        .GetByUserIdAsync(
                            user.Id,
                            cancellationToken);


                if (application is null)
                {
                    await RecordLoginAttemptAsync(
                        user.Id,
                        email,
                        false,
                        cancellationToken);


                    throw new ForbiddenException(
                        "CLAIMS_MANAGER_APPLICATION_NOT_FOUND",
                        "Claims Manager application was not found.");
                }


                if (application.Status ==
                        ClaimsManagerApprovalStatus.PendingApproval ||
                    application.Status ==
                        ClaimsManagerApprovalStatus.Rejected)
                {
                    var resetRestrictedResult =
                        await _userManager
                            .ResetAccessFailedCountAsync(user);


                    if (!resetRestrictedResult.Succeeded)
                    {
                        throw new BadRequestException(
                            "LOGIN_COUNTER_RESET_FAILED",
                            "Login could not be completed.");
                    }


                    await RecordLoginAttemptAsync(
                        user.Id,
                        email,
                        true,
                        cancellationToken);


                    await AddAuditLogAsync(
                        nameof(ApplicationUser),
                        user.Id,
                        "ClaimsManagerRestrictedAccessIssued",
                        user.Id,
                        null,
                        application.Status.ToString(),
                        cancellationToken);


                    return new LoginResponse
                    {
                        RequiresClaimsManagerAction = true,


                        ClaimsManagerApplicationToken =
                            _jwtTokenService
                                .GenerateClaimsManagerApplicationToken(
                                    user,
                                    application.Status),


                        ClaimsManagerApplicationStatus =
                            application.Status.ToString(),


                        RejectionReason =
                            application.Status ==
                            ClaimsManagerApprovalStatus.Rejected
                                ? application.RejectionReason
                                : null
                    };
                }
            }


            var resetResult =
                await _userManager
                    .ResetAccessFailedCountAsync(user);


            if (!resetResult.Succeeded)
            {
                throw new BadRequestException(
                    "LOGIN_COUNTER_RESET_FAILED",
                    "Login could not be completed.");
            }


            if (await _userManager
                .GetTwoFactorEnabledAsync(user))
            {
                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    user.Id,
                    "MfaChallengeIssued",
                    user.Id,
                    null,
                    null,
                    cancellationToken);


                return new LoginResponse
                {
                    RequiresMfa = true,
                    MfaChallengeToken =
                        _jwtTokenService
                            .GenerateMfaChallengeToken(user)
                };
            }


            return await CompleteLoginAsync(
                user,
                roles,
                email,
                cancellationToken);
        }
        public async Task<LoginResponse> VerifyMfaAsync(
            VerifyMfaRequest request,
            CancellationToken cancellationToken)
        {
            var userId=
            _jwtTokenService
                .ValidateMfaChallengeToken(
                    request.MfaChallengeToken);


            var user =
                await _userManager.FindByIdAsync(
                    userId);


            if (user is null ||
                user.Status != UserStatus.Active ||
                !await _userManager
                    .GetTwoFactorEnabledAsync(user))
            {
                throw new UnauthorizedException(
                    "INVALID_MFA_CHALLENGE",
                    "Invalid or expired MFA challenge.");
            }


            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new ForbiddenException(
                    "ACCOUNT_LOCKED",
                    "Account is locked. Try again later.");
            }


            var code =
                NormalizeAuthenticatorCode(
                    request.Code);


            var codeValid =
                await _userManager
                    .VerifyTwoFactorTokenAsync(
                        user,
                        TokenOptions
                            .DefaultAuthenticatorProvider,
                        code);


            if (!codeValid)
            {
                var failedResult =
                    await _userManager
                        .AccessFailedAsync(user);


                await AddAuditLogAsync(
                    nameof(ApplicationUser),
                    user.Id,
                    "MfaVerificationFailed",
                    user.Id,
                    null,
                    null,
                    cancellationToken);


                if (!failedResult.Succeeded)
                {
                    throw new BadRequestException(
                        "MFA_FAILURE_UPDATE_FAILED",
                        "MFA verification could not be processed.");
                }


                throw new UnauthorizedException(
                    "INVALID_MFA_CODE",
                    "Invalid authenticator code.");
            }


            var roles =
                await _userManager.GetRolesAsync(user);


            await EnsureRoleEligibilityAsync(
                user,
                roles,
                cancellationToken);


            var resetResult =
                await _userManager
                    .ResetAccessFailedCountAsync(user);


            if (!resetResult.Succeeded)
            {
                throw new BadRequestException(
                    "LOGIN_COUNTER_RESET_FAILED",
                    "Login could not be completed.");
            }


            var email =
                user.Email ??
                user.UserName ??
                user.Id;


            return await CompleteLoginAsync(
                user,
                roles,
                email,
                cancellationToken);
        }


        public async Task<MfaSetupResponse>
            BeginMfaSetupAsync(
                string userId,
                CancellationToken cancellationToken)
        {
            var user =
                await GetUserOrThrowAsync(userId);


            if (await _userManager
                .GetTwoFactorEnabledAsync(user))
            {
                throw new BadRequestException(
                    "MFA_ALREADY_ENABLED",
                    "MFA is already enabled.");
            }


            var key =
                await _userManager
                    .GetAuthenticatorKeyAsync(user);


            if (string.IsNullOrWhiteSpace(key))
            {
                var resetResult =
                    await _userManager
                        .ResetAuthenticatorKeyAsync(user);


                if (!resetResult.Succeeded)
                {
                    throw CreateIdentityBadRequest(
                        "MFA_KEY_GENERATION_FAILED",
                        resetResult);
                }


                key =
                    await _userManager
                        .GetAuthenticatorKeyAsync(user);
            }


            if (string.IsNullOrWhiteSpace(key))
            {
                throw new BadRequestException(
                    "MFA_KEY_GENERATION_FAILED",
                    "Unable to generate an authenticator key.");
            }


            var email =
                user.Email ??
                user.UserName ??
                user.Id;


            await AddAuditLogAsync(
                nameof(ApplicationUser),
                user.Id,
                "MfaSetupStarted",
                user.Id,
                null,
                null,
                cancellationToken);


            return new MfaSetupResponse
            {
                SharedKey = FormatAuthenticatorKey(key),
                AuthenticatorUri =
                    GenerateAuthenticatorUri(
                        email,
                        key)
            };
        }


        public async Task ConfirmMfaSetupAsync(
            string userId,
            ConfirmMfaSetupRequest request,
            CancellationToken cancellationToken)
        {
            var user =
                await GetUserOrThrowAsync(userId);


            if (await _userManager
                .GetTwoFactorEnabledAsync(user))
            {
                throw new BadRequestException(
                    "MFA_ALREADY_ENABLED",
                    "MFA is already enabled.");
            }


            var code =
                NormalizeAuthenticatorCode(
                    request.Code);


            var valid =
                await _userManager
                    .VerifyTwoFactorTokenAsync(
                        user,
                        TokenOptions
                            .DefaultAuthenticatorProvider,
                        code);


            if (!valid)
            {
                throw new BadRequestException(
                    "INVALID_MFA_CODE",
                    "Invalid authenticator code.");
            }


            var enableResult =
                await _userManager
                    .SetTwoFactorEnabledAsync(
                        user,
                        true);


            if (!enableResult.Succeeded)
            {
                throw CreateIdentityBadRequest(
                    "MFA_ENABLE_FAILED",
                    enableResult);
            }


            user.RefreshTokenVersion++;
            user.UpdatedAtUtc = DateTime.UtcNow;


            var updateResult =
                await _userManager.UpdateAsync(user);


            if (!updateResult.Succeeded)
            {
                throw CreateIdentityBadRequest(
                    "MFA_ENABLE_FAILED",
                    updateResult);
            }


            await RevokeAllSessionsAsync(
                user.Id,
                cancellationToken);


            await AddAuditLogAsync(
                nameof(ApplicationUser),
                user.Id,
                "MfaEnabled",
                user.Id,
                null,
                null,
                cancellationToken);
        }


        public async Task DisableMfaAsync(
            string userId,
            ConfirmMfaSetupRequest request,
            CancellationToken cancellationToken)
        {
            var user =
                await GetUserOrThrowAsync(userId);


            if (!await _userManager
                .GetTwoFactorEnabledAsync(user))
            {
                throw new BadRequestException(
                    "MFA_NOT_ENABLED",
                    "MFA is not enabled.");
            }


            var valid =
                await _userManager
                    .VerifyTwoFactorTokenAsync(
                        user,
                        TokenOptions
                            .DefaultAuthenticatorProvider,
                        NormalizeAuthenticatorCode(
                            request.Code));


            if (!valid)
            {
                throw new UnauthorizedException(
                    "INVALID_MFA_CODE",
                    "Invalid authenticator code.");
            }


            var disableResult =
                await _userManager
                    .SetTwoFactorEnabledAsync(
                        user,
                        false);


            if (!disableResult.Succeeded)
            {
                throw CreateIdentityBadRequest(
                    "MFA_DISABLE_FAILED",
                    disableResult);
            }


            var resetResult =
                await _userManager
                    .ResetAuthenticatorKeyAsync(user);


            if (!resetResult.Succeeded)
            {
                throw CreateIdentityBadRequest(
                    "MFA_KEY_RESET_FAILED",
                    resetResult);
            }


            user.RefreshTokenVersion++;
            user.UpdatedAtUtc = DateTime.UtcNow;


            var updateResult =
                await _userManager.UpdateAsync(user);


            if (!updateResult.Succeeded)
            {
                throw CreateIdentityBadRequest(
                    "MFA_DISABLE_FAILED",
                    updateResult);
            }


            await RevokeAllSessionsAsync(
                user.Id,
                cancellationToken);


            await AddAuditLogAsync(
                nameof(ApplicationUser),
                user.Id,
                "MfaDisabled",
                user.Id,
                null,
                null,
                cancellationToken);
        }


        public async Task<LoginResponse> RefreshTokenAsync(
            RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var tokenHash =
                SecurityHelper.ComputeSha256(
                    request.RefreshToken);


            var storedToken =
                await _refreshTokenRepository
                    .GetByHashAsync(
                        tokenHash,
                        cancellationToken);


            if (storedToken is null ||
                storedToken.RevokedAtUtc.HasValue ||
                storedToken.ExpiresAtUtc <=
                    DateTime.UtcNow)
            {
                throw InvalidRefreshToken();
            }


            var user =
                await _userManager.FindByIdAsync(
                    storedToken.UserId);


            if (user is null ||
                user.Status != UserStatus.Active ||
                storedToken.TokenVersion !=
                    user.RefreshTokenVersion)
            {
                throw InvalidRefreshToken();
            }


            var roles =
                await _userManager.GetRolesAsync(
                    user);


            await EnsureRoleEligibilityAsync(
                user,
                roles,
                cancellationToken);


            var issuedAtUtc =
                DateTime.UtcNow;


            var rawNewToken =
                _jwtTokenService
                    .GenerateRefreshToken();


            var newRefreshToken =
                new RefreshToken
                {
                    Id = Guid.NewGuid(),


                    UserId = user.Id,


                    TokenHash =
                        SecurityHelper.ComputeSha256(
                            rawNewToken),


                    TokenVersion =
                        user.RefreshTokenVersion,


                    CreatedAtUtc =
                        issuedAtUtc,


                    ExpiresAtUtc =
                        issuedAtUtc.AddDays(
                            _jwtSettings
                                .RefreshTokenExpirationDays)
                };


            try
            {
                await _authUnitOfWork
                    .ExecuteInTransactionAsync(
                        async transactionToken =>
                        {
                            /*
                             * Revoke the token used for this request.
                             */
                            storedToken.RevokedAtUtc =
                                issuedAtUtc;


                            await _refreshTokenRepository
                                .UpdateAsync(
                                    storedToken,
                                    transactionToken);


                            /*
                             * Store only the hash of the replacement
                             * refresh token.
                             */
                            await _refreshTokenRepository
                                .AddAsync(
                                    newRefreshToken,
                                    transactionToken);


                            /*
                             * Save the audit record in the same database
                             * transaction as the rotation.
                             */
                            await AddAuditLogWithoutSaveAsync(
                                nameof(RefreshToken),
                                storedToken.Id.ToString(),
                                "RefreshTokenRotated",
                                user.Id,
                                null,
                                null,
                                transactionToken);
                        },
                        cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                /*
                 * Another request already rotated or revoked the same
                 * refresh token.
                 */
                throw new UnauthorizedException(
                    "REFRESH_TOKEN_ALREADY_USED",
                    "The refresh token has already been used.");
            }
            catch (DbUpdateException)
            {
                throw new BadRequestException(
                    "REFRESH_TOKEN_ROTATION_FAILED",
                    "The session could not be refreshed.");
            }


            var accessToken =
                _jwtTokenService.GenerateAccessToken(
                    user,
                    roles);


            return new LoginResponse
            {
                AccessToken =
                    accessToken,


                RefreshToken =
                    rawNewToken,


                ExpiresAtUtc =
                    issuedAtUtc.AddMinutes(
                        _jwtSettings
                            .AccessTokenExpirationMinutes),


                RequiresMfa =
                    false,


                MfaChallengeToken =
                    null,


                RequiresClaimsManagerAction =
                    false,


                ClaimsManagerApplicationToken =
                    null,


                ClaimsManagerApplicationStatus =
                    null,


                RejectionReason =
                    null
            };
        }
        public async Task LogoutAsync(
            string refreshToken,
            CancellationToken cancellationToken)
        {
            var tokenHash =
                SecurityHelper.ComputeSha256(
                    refreshToken);


            var token =
                await _refreshTokenRepository
                    .GetByHashAsync(
                        tokenHash,
                        cancellationToken);


            if (token is null ||
                token.RevokedAtUtc.HasValue)
            {
                return;
            }


            token.RevokedAtUtc = DateTime.UtcNow;


            await _refreshTokenRepository.UpdateAsync(
                token,
                cancellationToken);


            await _refreshTokenRepository
                .SaveChangesAsync(
                    cancellationToken);


            await AddAuditLogAsync(
                nameof(RefreshToken),
                token.Id.ToString(),
                "UserLoggedOut",
                token.UserId,
                null,
                null,
                cancellationToken);
        }


        public async Task ForgotPasswordAsync(
            ForgotPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var email =
                InputValidationHelper.NormalizeEmail(
                    request.Email);


            var user =
                await _userManager.FindByEmailAsync(
                    email);


            if (user is null)
            {
                return;
            }


            var now = DateTime.UtcNow;


            var activeTokens =
                await _passwordResetTokenRepository
                    .GetActiveTokensByUserIdAsync(
                        user.Id,
                        cancellationToken);


            foreach (var token in activeTokens)
            {
                token.UsedAtUtc = now;


                await _passwordResetTokenRepository
                    .UpdateAsync(
                        token,
                        cancellationToken);
            }


            var rawToken =
                _jwtTokenService
                    .GenerateRefreshToken();


            await _passwordResetTokenRepository
                .AddAsync(
                    new PasswordResetToken
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        TokenHash =
                            SecurityHelper
                                .ComputeSha256(
                                    rawToken),
                        ExpiresAtUtc =
                            now.AddHours(1)
                    },
                    cancellationToken);


            await _passwordResetTokenRepository
                .SaveChangesAsync(
                    cancellationToken);


            await _passwordResetNotificationService
                .SendPasswordResetAsync(
                    email,
                    rawToken,
                    cancellationToken);


            await AddAuditLogAsync(
                nameof(ApplicationUser),
                user.Id,
                "PasswordResetRequested",
                user.Id,
                null,
                null,
                cancellationToken);
        }


        public async Task ResetPasswordAsync(
           ResetPasswordRequest request,
           CancellationToken cancellationToken)
        {
            var email =
                InputValidationHelper.NormalizeEmail(
                    request.Email);


            var user =
                await _userManager.FindByEmailAsync(email);


            if (user is null)
            {
                throw InvalidPasswordReset();
            }


            var resetToken =
                await _passwordResetTokenRepository
                    .GetByHashAndUserIdAsync(
                        SecurityHelper.ComputeSha256(
                            request.Token.Trim()),
                        user.Id,
                        cancellationToken);


            if (resetToken is null ||
                resetToken.UsedAtUtc.HasValue ||
                resetToken.ExpiresAtUtc <=
                    DateTime.UtcNow)
            {
                throw InvalidPasswordReset();
            }


            await _authUnitOfWork.ExecuteInTransactionAsync(
                async transactionToken =>
                {
                    var identityToken =
                        await _userManager
                            .GeneratePasswordResetTokenAsync(
                                user);


                    var resetResult =
                        await _userManager.ResetPasswordAsync(
                            user,
                            identityToken,
                            request.NewPassword);


                    if (!resetResult.Succeeded)
                    {
                        throw CreateIdentityBadRequest(
                            "PASSWORD_RESET_FAILED",
                            resetResult);
                    }


                    var now = DateTime.UtcNow;


                    resetToken.UsedAtUtc = now;


                    await _passwordResetTokenRepository
                        .UpdateAsync(
                            resetToken,
                            transactionToken);


                    user.RefreshTokenVersion++;
                    user.UpdatedAtUtc = now;


                    var userUpdateResult =
                        await _userManager.UpdateAsync(user);


                    if (!userUpdateResult.Succeeded)
                    {
                        throw CreateIdentityBadRequest(
                            "PASSWORD_RESET_FAILED",
                            userUpdateResult);
                    }


                    await _refreshTokenRepository
                        .RevokeAllActiveByUserIdAsync(
                            user.Id,
                            transactionToken);


                    await AddAuditLogWithoutSaveAsync(
                        nameof(ApplicationUser),
                        user.Id,
                        "PasswordResetCompleted",
                        user.Id,
                        null,
                        null,
                        transactionToken);
                },
                cancellationToken);
        }


        public async Task ChangePasswordAsync(
            string userId,
            ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            var user =
                await GetUserOrThrowAsync(userId);


            if (request.CurrentPassword ==
                request.NewPassword)
            {
                throw new BadRequestException(
                    "PASSWORD_UNCHANGED",
                    "New password must be different from current password.");
            }


            await _authUnitOfWork.ExecuteInTransactionAsync(
                async transactionToken =>
                {
                    var changeResult =
                        await _userManager.ChangePasswordAsync(
                            user,
                            request.CurrentPassword,
                            request.NewPassword);


                    if (!changeResult.Succeeded)
                    {
                        throw CreateIdentityBadRequest(
                            "PASSWORD_CHANGE_FAILED",
                            changeResult);
                    }


                    user.RefreshTokenVersion++;
                    user.UpdatedAtUtc =
                        DateTime.UtcNow;


                    var updateResult =
                        await _userManager.UpdateAsync(user);


                    if (!updateResult.Succeeded)
                    {
                        throw CreateIdentityBadRequest(
                            "PASSWORD_CHANGE_FAILED",
                            updateResult);
                    }


                    await _refreshTokenRepository
                        .RevokeAllActiveByUserIdAsync(
                            user.Id,
                            transactionToken);


                    await AddAuditLogWithoutSaveAsync(
                        nameof(ApplicationUser),
                        user.Id,
                        "PasswordChanged",
                        user.Id,
                        null,
                        null,
                        transactionToken);
                },
                cancellationToken);
        }
        public async Task ApproveClaimsManagerAsync(
            Guid applicationId,
            string adminUserId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _authUnitOfWork
                    .ExecuteInTransactionAsync(
                        async transactionToken =>
                        {
                            var application =
                                await _claimsManagerRepository
                                    .GetByIdAsync(
                                        applicationId,
                                        transactionToken);


                            if (application is null)
                            {
                                throw new NotFoundException(
                                    "CLAIMS_MANAGER_APPLICATION_NOT_FOUND",
                                    "Claims Manager application was not found.");
                            }


                            if (application.Status !=
                                ClaimsManagerApprovalStatus
                                    .PendingApproval)
                            {
                                throw new BadRequestException(
                                    "CLAIMS_MANAGER_APPLICATION_NOT_PENDING",
                                    "Only a pending Claims Manager application can be approved.");
                            }


                            var user =
                                await _userManager
                                    .FindByIdAsync(
                                        application.UserId);


                            if (user is null)
                            {
                                throw new NotFoundException(
                                    "CLAIMS_MANAGER_USER_NOT_FOUND",
                                    "Claims Manager user was not found.");
                            }


                            if (user.Status ==
                                UserStatus.Inactive)
                            {
                                throw new BadRequestException(
                                    "CLAIMS_MANAGER_ACCOUNT_INACTIVE",
                                    "Reactivate the account before approving the application.");
                            }


                            /*
                             * Invalidate restricted application tokens
                             * and any existing sessions because the
                             * user's security state is changing.
                             */
                            user.RefreshTokenVersion++;


                            user.UpdatedAtUtc =
                                DateTime.UtcNow;


                            var userUpdateResult =
                                await _userManager
                                    .UpdateAsync(user);


                            if (!userUpdateResult.Succeeded)
                            {
                                throw CreateIdentityBadRequest(
                                    "CLAIMS_MANAGER_APPROVAL_FAILED",
                                    userUpdateResult);
                            }


                            await _refreshTokenRepository
                                .RevokeAllActiveByUserIdAsync(
                                    user.Id,
                                    transactionToken);


                            application.Status =
                                ClaimsManagerApprovalStatus
                                    .Approved;


                            application.ApprovedAtUtc =
                                DateTime.UtcNow;


                            application.ApprovedByUserId =
                                adminUserId;


                            application.RejectionReason =
                                null;


                            await _claimsManagerRepository
                                .UpdateAsync(
                                    application,
                                    transactionToken);


                            await AddAuditLogWithoutSaveAsync(
                                nameof(
                                    ClaimsManagerApplication),
                                application.Id.ToString(),
                                "ClaimsManagerApproved",
                                adminUserId,
                                ClaimsManagerApprovalStatus
                                    .PendingApproval
                                    .ToString(),
                                ClaimsManagerApprovalStatus
                                    .Approved
                                    .ToString(),
                                transactionToken);
                        },
                        cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BadRequestException(
                    "CLAIMS_MANAGER_DECISION_CONFLICT",
                    "The application was modified by another request.");
            }
        }
        public async Task RejectClaimsManagerAsync(
            Guid applicationId,
            RejectClaimsManagerRequest request,
            string adminUserId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _authUnitOfWork
                    .ExecuteInTransactionAsync(
                        async transactionToken =>
                        {
                            var application =
                                await _claimsManagerRepository
                                    .GetByIdAsync(
                                        applicationId,
                                        transactionToken);


                            if (application is null)
                            {
                                throw new NotFoundException(
                                    "CLAIMS_MANAGER_APPLICATION_NOT_FOUND",
                                    "Claims Manager application was not found.");
                            }


                            if (application.Status !=
                                ClaimsManagerApprovalStatus
                                    .PendingApproval)
                            {
                                throw new BadRequestException(
                                    "CLAIMS_MANAGER_APPLICATION_NOT_PENDING",
                                    "Only a pending Claims Manager application can be rejected.");
                            }


                            var user =
                                await _userManager
                                    .FindByIdAsync(
                                        application.UserId);


                            if (user is null)
                            {
                                throw new NotFoundException(
                                    "CLAIMS_MANAGER_USER_NOT_FOUND",
                                    "Claims Manager user was not found.");
                            }


                            /*
                             * Invalidate the restricted token carrying
                             * the old PendingApproval security state.
                             */
                            user.RefreshTokenVersion++;


                            user.UpdatedAtUtc =
                                DateTime.UtcNow;


                            var userUpdateResult =
                                await _userManager
                                    .UpdateAsync(user);


                            if (!userUpdateResult.Succeeded)
                            {
                                throw CreateIdentityBadRequest(
                                    "CLAIMS_MANAGER_REJECTION_FAILED",
                                    userUpdateResult);
                            }


                            await _refreshTokenRepository
                                .RevokeAllActiveByUserIdAsync(
                                    user.Id,
                                    transactionToken);


                            application.Status =
                                ClaimsManagerApprovalStatus
                                    .Rejected;


                            application.ApprovedAtUtc =
                                null;


                            application.ApprovedByUserId =
                                adminUserId;


                            application.RejectionReason =
                                InputValidationHelper
                                    .NormalizeText(
                                        request.Reason);


                            await _claimsManagerRepository
                                .UpdateAsync(
                                    application,
                                    transactionToken);


                            await AddAuditLogWithoutSaveAsync(
                                nameof(
                                    ClaimsManagerApplication),
                                application.Id.ToString(),
                                "ClaimsManagerRejected",
                                adminUserId,
                                ClaimsManagerApprovalStatus
                                    .PendingApproval
                                    .ToString(),
                                ClaimsManagerApprovalStatus
                                    .Rejected
                                    .ToString(),
                                transactionToken);
                        },
                        cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BadRequestException(
                    "CLAIMS_MANAGER_DECISION_CONFLICT",
                    "The application was modified by another request.");
            }
        }
        public async Task UpdateUserStatusAsync(
           string userId,
           UpdateUserStatusRequest request,
           string adminUserId,
           CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new BadRequestException(
                    "INVALID_USER_ID",
                    "User identifier is required.");
            }


            if (string.IsNullOrWhiteSpace(adminUserId))
            {
                throw new UnauthorizedException(
                    "ADMIN_USER_ID_REQUIRED",
                    "The authenticated administrator identifier is missing.");
            }


            var user =
                await GetUserOrThrowAsync(
                    userId.Trim());


            var requestedStatus =
                request.IsActive
                    ? UserStatus.Active
                    : UserStatus.Inactive;


            var roles =
                await _userManager.GetRolesAsync(
                    user);


            var isCustomer =
                roles.Contains(
                    Roles.Customer,
                    StringComparer.OrdinalIgnoreCase);


            var synchronizationReason =
                request.IsActive
                    ? "Customer account activated by administrator."
                    : "Customer account deactivated by administrator.";


            /*
             * Idempotent request:
             *
             * Auth status already matches the requested status.
             * Customer synchronization is attempted again for Customer
             * accounts in case a previous synchronization failed.
             */
            if (user.Status == requestedStatus)
            {
                if (isCustomer)
                {
                    await SynchronizeCustomerStatusAsync(
                        authUserId:
                            user.Id,
                        status:
                            requestedStatus.ToString(),
                        reason:
                            synchronizationReason,
                        cancellationToken:
                            cancellationToken);
                }


                return;
            }


            var oldStatus =
                user.Status;


            var utcNow =
                DateTime.UtcNow;


            user.Status =
                requestedStatus;


            user.UpdatedAtUtc =
                utcNow;


            /*
             * Invalidate all existing access/refresh-token sessions when
             * an account is deactivated.
             */
            if (requestedStatus ==
                UserStatus.Inactive)
            {
                user.RefreshTokenVersion++;
            }


            var updateResult =
                await _userManager.UpdateAsync(
                    user);


            if (!updateResult.Succeeded)
            {
                throw CreateIdentityBadRequest(
                    "USER_STATUS_UPDATE_FAILED",
                    updateResult);
            }


            if (requestedStatus ==
                UserStatus.Inactive)
            {
                await RevokeAllSessionsAsync(
                    user.Id,
                    cancellationToken);
            }


            await AddAuditLogAsync(
                nameof(ApplicationUser),
                user.Id,
                request.IsActive
                    ? "UserActivated"
                    : "UserDeactivated",
                adminUserId.Trim(),
                oldStatus.ToString(),
                requestedStatus.ToString(),
                cancellationToken);


            /*
             * Only Customer accounts exist in Customer Service.
             *
             * Claims Managers and Admins must not be synchronized to
             * Customer Service.
             */
            if (isCustomer)
            {
                await SynchronizeCustomerStatusAsync(
                    authUserId:
                        user.Id,
                    status:
                        requestedStatus.ToString(),
                    reason:
                        synchronizationReason,
                    cancellationToken:
                        cancellationToken);
            }
        }


        public async Task<
            IReadOnlyCollection<
                ActiveClaimsManagerResponse>>
            GetActiveClaimsManagersAsync(
                CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();


            IList<ApplicationUser> claimsManagerUsers =
                await _userManager
                    .GetUsersInRoleAsync(
                        Roles.ClaimsManager);


            var activeClaimsManagers =
                new List<
                    ActiveClaimsManagerResponse>();


            foreach (
                ApplicationUser user
                in claimsManagerUsers)
            {
                cancellationToken
                    .ThrowIfCancellationRequested();


                if (
                    user.Status !=
                    UserStatus.Active
                )
                {
                    continue;
                }


                bool isLockedOut =
                    await _userManager
                        .IsLockedOutAsync(
                            user);


                if (isLockedOut)
                {
                    continue;
                }


                ClaimsManagerApplication?
                    application =
                        await _claimsManagerRepository
                            .GetByUserIdAsync(
                                user.Id,
                                cancellationToken);


                if (
                    application is null ||
                    application.Status !=
                        ClaimsManagerApprovalStatus
                            .Approved
                )
                {
                    continue;
                }


                string email =
                    user.Email ??
                    user.UserName ??
                    string.Empty;


                if (
                    string.IsNullOrWhiteSpace(
                        email)
                )
                {
                    continue;
                }


                activeClaimsManagers.Add(
                    new ActiveClaimsManagerResponse
                    {
                        UserId =
                            user.Id,


                        FirstName =
                            user.FirstName?.Trim() ??
                            string.Empty,


                        LastName =
                            user.LastName?.Trim() ??
                            string.Empty,


                        Email =
                            email.Trim(),


                        YearsOfExperience =
                            application
                                .YearsOfExperience,


                        ProfessionalSummary =
                            application
                                .ProfessionalSummary
                                ?.Trim() ??
                            string.Empty
                    });
            }


            return activeClaimsManagers
                .OrderBy(
                    claimsManager =>
                        claimsManager.FirstName,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    claimsManager =>
                        claimsManager.LastName,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    claimsManager =>
                        claimsManager.Email,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }


        public async Task<
            List<PendingClaimsManagerResponse>>
            GetPendingClaimsManagersAsync(
                CancellationToken cancellationToken)
        {
            var applications =
                await _claimsManagerRepository
                    .GetPendingAsync(
                        cancellationToken);


            return applications
                .Select(
                    application =>
                        new PendingClaimsManagerResponse
                        {
                            ApplicationId =
                                application.Id,


                            YearsOfExperience =
                                application
                                    .YearsOfExperience,


                            ProfessionalSummary =
                                application
                                    .ProfessionalSummary,


                            SubmittedAtUtc =
                                application.CreatedAtUtc
                        })
                .ToList();
        }
        public async Task<
    ActiveClaimsManagerResponse?>
    GetActiveClaimsManagerAsync(
        string userId,
        CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(
                    userId))
            {
                throw new BadRequestException(
                    "CLAIMS_MANAGER_USER_ID_REQUIRED",
                    "Claims Manager user identifier is required.");
            }


            cancellationToken
                .ThrowIfCancellationRequested();


            ApplicationUser? user =
                await _userManager.FindByIdAsync(
                    userId.Trim());


            if (
                user is null ||
                user.Status != UserStatus.Active
            )
            {
                return null;
            }


            bool isClaimsManager =
                await _userManager.IsInRoleAsync(
                    user,
                    Roles.ClaimsManager);


            if (!isClaimsManager)
            {
                return null;
            }


            bool isLockedOut =
                await _userManager.IsLockedOutAsync(
                    user);


            if (isLockedOut)
            {
                return null;
            }


            ClaimsManagerApplication? application =
                await _claimsManagerRepository
                    .GetByUserIdAsync(
                        user.Id,
                        cancellationToken);


            if (
                application is null ||
                application.Status !=
                    ClaimsManagerApprovalStatus.Approved
            )
            {
                return null;
            }


            string email =
                user.Email ??
                user.UserName ??
                string.Empty;


            if (string.IsNullOrWhiteSpace(
                    email))
            {
                return null;
            }


            return new ActiveClaimsManagerResponse
            {
                UserId =
                    user.Id,


                FirstName =
                    user.FirstName?.Trim() ??
                    string.Empty,


                LastName =
                    user.LastName?.Trim() ??
                    string.Empty,


                Email =
                    email.Trim(),


                YearsOfExperience =
                    application.YearsOfExperience,


                ProfessionalSummary =
                    application
                        .ProfessionalSummary
                        ?.Trim() ??
                    string.Empty
            };
        }

        public async Task<InternalCustomerContactResponse?>
            GetInternalCustomerContactAsync(
                string userId,
                CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new BadRequestException(
                    "CUSTOMER_USER_ID_REQUIRED",
                    "Customer user identifier is required.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            ApplicationUser? user =
                await _userManager.FindByIdAsync(
                    userId.Trim());

            if (user is null)
            {
                return null;
            }

            return new InternalCustomerContactResponse
            {
                Email = user.Email?.Trim() ?? string.Empty
            };
        }




        public async Task<
            ClaimsManagerApplicationStatusResponse>
            GetClaimsManagerApplicationStatusByTokenAsync(
                string applicationToken,
                CancellationToken cancellationToken)
        {
            var userId =
                _jwtTokenService
                    .ValidateClaimsManagerApplicationToken(
                        applicationToken);


            var user =
                await _userManager.FindByIdAsync(userId);


            if (user is null ||
                user.Status != UserStatus.Active)
            {
                throw InvalidClaimsManagerApplicationToken();
            }


            var isClaimsManager =
                await _userManager.IsInRoleAsync(
                    user,
                    Roles.ClaimsManager);


            if (!isClaimsManager)
            {
                throw InvalidClaimsManagerApplicationToken();
            }


            var application =
                await _claimsManagerRepository
                    .GetByUserIdAsync(
                        userId,
                        cancellationToken);


            if (application is null)
            {
                throw new NotFoundException(
                    "CLAIMS_MANAGER_APPLICATION_NOT_FOUND",
                    "Claims Manager application was not found.");
            }


            if (application.Status ==
                ClaimsManagerApprovalStatus.Approved)
            {
                throw new ForbiddenException(
                    "CLAIMS_MANAGER_RESTRICTED_ACCESS_NOT_ALLOWED",
                    "Restricted application access is not available for an approved Claims Manager.");
            }


            return new ClaimsManagerApplicationStatusResponse
            {
                ApplicationId =
                    application.Id,


                Status =
                    application.Status.ToString(),


                RejectionReason =
                    application.RejectionReason,


                SubmittedAtUtc =
                    application.CreatedAtUtc,


                ApprovedAtUtc =
                    application.ApprovedAtUtc
            };
        }


        public async Task<StoredFileContent>
            GetVerificationDocumentAsync(
                Guid applicationId,
                string adminUserId,
                CancellationToken cancellationToken)
        {
            var application =
                await _claimsManagerRepository
                    .GetByIdAsync(
                        applicationId,
                        cancellationToken);


            if (application is null)
            {
                throw new NotFoundException(
                    "CLAIMS_MANAGER_APPLICATION_NOT_FOUND",
                    "Claims Manager application was not found.");
            }


            var file =
                await _fileStorageService
                    .OpenClaimsManagerDocumentAsync(
                        application
                            .VerificationDocumentPath,
                        application.Id,
                        cancellationToken);


            await AddAuditLogAsync(
                "VerificationDocument",
                application.Id.ToString(),
                "DocumentDownloaded",
                adminUserId,
                null,
                null,
                cancellationToken);


            return file;
        }


        private async Task<LoginResponse> CompleteLoginAsync(
            ApplicationUser user,
            IList<string> roles,
            string email,
            CancellationToken cancellationToken)
        {
            var issuedAtUtc =
                DateTime.UtcNow;


            var accessToken =
                _jwtTokenService.GenerateAccessToken(
                    user,
                    roles);


            var rawRefreshToken =
                _jwtTokenService.GenerateRefreshToken();


            var refreshToken =
                new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TokenHash =
                        SecurityHelper.ComputeSha256(
                            rawRefreshToken),
                    TokenVersion =
                        user.RefreshTokenVersion,
                    CreatedAtUtc =
                        issuedAtUtc,
                    ExpiresAtUtc =
                        issuedAtUtc.AddDays(
                            _jwtSettings
                                .RefreshTokenExpirationDays)
                };


            await _authUnitOfWork.ExecuteInTransactionAsync(
                async transactionToken =>
                {
                    await _refreshTokenRepository.AddAsync(
                        refreshToken,
                        transactionToken);


                    await AddAuditLogWithoutSaveAsync(
                        nameof(ApplicationUser),
                        user.Id,
                        "LoginSuccess",
                        user.Id,
                        null,
                        null,
                        transactionToken);
                },
                cancellationToken);


            await RecordLoginAttemptAsync(
                user.Id,
                email,
                true,
                cancellationToken);


            return new LoginResponse
            {
                AccessToken =
                    accessToken,


                RefreshToken =
                    rawRefreshToken,


                ExpiresAtUtc =
                    issuedAtUtc.AddMinutes(
                        _jwtSettings
                            .AccessTokenExpirationMinutes),


                RequiresMfa =
                    false,


                MfaChallengeToken =
                    null,


                RequiresClaimsManagerAction =
                    false,


                ClaimsManagerApplicationToken =
                    null,


                ClaimsManagerApplicationStatus =
                    null,


                RejectionReason =
                    null
            };
        }


        private async Task EnsureRoleEligibilityAsync(
            ApplicationUser user,
            IList<string> roles,
            CancellationToken cancellationToken)
        {
            if (!roles.Contains(
                    Roles.ClaimsManager))
            {
                return;
            }


            var application =
                await _claimsManagerRepository
                    .GetByUserIdAsync(
                        user.Id,
                        cancellationToken);


            if (application is null)
            {
                throw new ForbiddenException(
                    "CLAIMS_MANAGER_APPLICATION_NOT_FOUND",
                    "Claims Manager application was not found.");
            }


            if (application.Status ==
                ClaimsManagerApprovalStatus
                    .PendingApproval)
            {
                throw new ForbiddenException(
                    "CLAIMS_MANAGER_PENDING_APPROVAL",
                    "Claims Manager account is pending approval.");
            }


            if (application.Status ==
                ClaimsManagerApprovalStatus.Rejected)
            {
                throw new ForbiddenException(
                    "CLAIMS_MANAGER_APPLICATION_REJECTED",
                    "Claims Manager application was rejected.");
            }
        }


        private async Task<ApplicationUser>
            GetUserOrThrowAsync(string userId)
        {
            var user =
                await _userManager.FindByIdAsync(
                    userId);


            if (user is null)
            {
                throw new NotFoundException(
                    "USER_NOT_FOUND",
                    "User was not found.");
            }


            return user;
        }


        private async Task RevokeAllSessionsAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            await _refreshTokenRepository
                .RevokeAllActiveByUserIdAsync(
                    userId,
                    cancellationToken);


            await _refreshTokenRepository
                .SaveChangesAsync(
                    cancellationToken);
        }




        private async Task RecordLoginAttemptAsync(
            string? userId,
            string email,
            bool success,
            CancellationToken cancellationToken)
        {
            await _loginAttemptRepository.AddAsync(
                new LoginAttempt
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Email = email,
                    Success = success,
                    AttemptedAtUtc = DateTime.UtcNow
                },
                cancellationToken);


            await _loginAttemptRepository
                .SaveChangesAsync(
                    cancellationToken);
        }


        private Task AddAuditLogWithoutSaveAsync(
            string entityName,
            string entityId,
            string action,
            string? performedBy,
            string? oldValues,
            string? newValues,
            CancellationToken cancellationToken)
        {
            return _auditLogRepository.AddAsync(
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    OldValues = oldValues,
                    NewValues = newValues,
                    PerformedBy = performedBy,
                    PerformedAtUtc =
                        DateTime.UtcNow
                },
                cancellationToken);
        }


        private async Task AddAuditLogAsync(
            string entityName,
            string entityId,
            string action,
            string? performedBy,
            string? oldValues,
            string? newValues,
            CancellationToken cancellationToken)
        {
            await AddAuditLogWithoutSaveAsync(
                entityName,
                entityId,
                action,
                performedBy,
                oldValues,
                newValues,
                cancellationToken);


            await _authUnitOfWork.SaveChangesAsync(
                cancellationToken);
        }
        private async Task TryDeleteDocumentAsync(
            string? path,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }


            try
            {
                await _fileStorageService
                    .DeleteClaimsManagerDocumentAsync(
                        path,
                        cancellationToken);
            }
            catch
            {
                // Cleanup failure must not obscure the original
                // registration or persistence failure.
            }
        }
        private static UnauthorizedException
            InvalidClaimsManagerApplicationToken()
        {
            return new UnauthorizedException(
                "INVALID_CLAIMS_MANAGER_APPLICATION_TOKEN",
                "Invalid or expired Claims Manager application token.");
        }


        private static string
            NormalizeAuthenticatorCode(
                string code)
        {
            return code
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty);
        }


        private static string
            FormatAuthenticatorKey(
                string key)
        {
            const int groupSize = 4;


            return string.Join(
                " ",
                Enumerable.Range(
                        0,
                        (key.Length +
                         groupSize - 1) /
                        groupSize)
                    .Select(index =>
                        key.Substring(
                            index * groupSize,
                            Math.Min(
                                groupSize,
                                key.Length -
                                index * groupSize)))
                    .Select(group =>
                        group.ToLowerInvariant()));
        }


        private static string
            GenerateAuthenticatorUri(
                string email,
                string key)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "otpauth://totp/{0}:{1}" +
                "?secret={2}&issuer={0}&digits=6",
                UrlEncoder.Default.Encode(
                    AuthenticatorIssuer),
                UrlEncoder.Default.Encode(email),
                key);
        }


        private static string
            JoinIdentityErrorCodes(
                IdentityResult result)
        {
            return string.Join(
                ", ",
                result.Errors.Select(
                    error => error.Code));
        }


        private static BadRequestException
            CreateIdentityBadRequest(
                string code,
                IdentityResult result)
        {
            return new BadRequestException(
                code,
                string.Join(
                    ", ",
                    result.Errors.Select(
                        error => error.Description)));
        }


        private static UnauthorizedException
            InvalidCredentials()
        {
            return new UnauthorizedException(
                "INVALID_CREDENTIALS",
                "Invalid credentials.");
        }


        private static UnauthorizedException
            InvalidRefreshToken()
        {
            return new UnauthorizedException(
                "INVALID_REFRESH_TOKEN",
                "Invalid or expired refresh token.");
        }


        private static BadRequestException
            InvalidPasswordReset()
        {
            return new BadRequestException(
                "INVALID_PASSWORD_RESET_REQUEST",
                "Invalid or expired password-reset request.");
        }
    }
}
