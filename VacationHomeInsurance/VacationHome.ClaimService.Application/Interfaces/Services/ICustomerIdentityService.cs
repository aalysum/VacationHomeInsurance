namespace VacationHome.ClaimService.Application
    .Interfaces.Services
{
    public interface ICustomerIdentityService
    {
        Task<Guid> GetCurrentCustomerIdAsync(
            CancellationToken cancellationToken);
    }
}
