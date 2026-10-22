namespace VacationHome.PolicyService.Application
    .Interfaces.Services
{
    public interface ICustomerIdentityService
    {
        Task<Guid> GetCurrentCustomerIdAsync(
            CancellationToken cancellationToken);
    }
}
