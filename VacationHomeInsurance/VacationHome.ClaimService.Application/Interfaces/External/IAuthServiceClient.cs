using VacationHome.ClaimService.Application.DTOs.External;


namespace VacationHome.ClaimService.Application
    .Interfaces.External
{
    public interface IAuthServiceClient
    {
        Task<ActiveClaimsManagerDto?>
            GetActiveClaimsManagerAsync(
                string claimsManagerId,
                CancellationToken cancellationToken = default);

        Task<InternalCustomerContactDto?>
            GetCustomerContactAsync(
                string userId,
                CancellationToken cancellationToken = default);
    }
}
