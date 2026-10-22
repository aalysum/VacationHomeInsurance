using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task RegisterCustomerAsync(
            RegisterCustomerRequest request,
            CancellationToken cancellationToken);


        Task RegisterClaimsManagerAsync(
            RegisterClaimsManagerRequest request,
            ClaimsManagerDocumentDto document,
            CancellationToken cancellationToken);


        Task ReapplyClaimsManagerAsync(
            string applicationToken,
            ReapplyClaimsManagerRequest request,
            ClaimsManagerDocumentDto document,
            CancellationToken cancellationToken);


        Task<LoginResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken);


        Task<LoginResponse> VerifyMfaAsync(
            VerifyMfaRequest request,
            CancellationToken cancellationToken);


        Task<LoginResponse> RefreshTokenAsync(
            RefreshTokenRequest request,
            CancellationToken cancellationToken);


        Task LogoutAsync(
            string refreshToken,
            CancellationToken cancellationToken);


        Task ForgotPasswordAsync(
            ForgotPasswordRequest request,
            CancellationToken cancellationToken);


        Task ResetPasswordAsync(
            ResetPasswordRequest request,
            CancellationToken cancellationToken);


        Task ChangePasswordAsync(
            string userId,
            ChangePasswordRequest request,
            CancellationToken cancellationToken);


        Task<MfaSetupResponse> BeginMfaSetupAsync(
            string userId,
            CancellationToken cancellationToken);


        Task ConfirmMfaSetupAsync(
            string userId,
            ConfirmMfaSetupRequest request,
            CancellationToken cancellationToken);


        Task DisableMfaAsync(
            string userId,
            ConfirmMfaSetupRequest request,
            CancellationToken cancellationToken);


        Task ApproveClaimsManagerAsync(
            Guid applicationId,
            string adminUserId,
            CancellationToken cancellationToken);


        Task RejectClaimsManagerAsync(
            Guid applicationId,
            RejectClaimsManagerRequest request,
            string adminUserId,
            CancellationToken cancellationToken);


        Task UpdateUserStatusAsync(
            string userId,
            UpdateUserStatusRequest request,
            string adminUserId,
            CancellationToken cancellationToken);


        Task<List<PendingClaimsManagerResponse>>
            GetPendingClaimsManagersAsync(
                CancellationToken cancellationToken);


        Task<ClaimsManagerApplicationStatusResponse>
            GetClaimsManagerApplicationStatusByTokenAsync(
                string applicationToken,
                CancellationToken cancellationToken);


        Task<StoredFileContent>
            GetVerificationDocumentAsync(
                Guid applicationId,
                string adminUserId,
                CancellationToken cancellationToken);
        Task<IReadOnlyCollection<
    ActiveClaimsManagerResponse>>
    GetActiveClaimsManagersAsync(
        CancellationToken cancellationToken);


        Task<ActiveClaimsManagerResponse?>
    GetActiveClaimsManagerAsync(
        string userId,
        CancellationToken cancellationToken);

        Task<InternalCustomerContactResponse?>
            GetInternalCustomerContactAsync(
                string userId,
                CancellationToken cancellationToken);

    }
}
