using VacationHome.PolicyService.Application.DTOs.External;
using VacationHome.PolicyService.Application.DTOs.Internal;


namespace VacationHome.PolicyService.Application.Interfaces.External
{
    public interface ICustomerServiceClient
    {
        Task<CustomerSnapshotDto?> GetCustomerAsync(
            Guid customerId,
            CancellationToken cancellationToken);


        Task<InternalPropertySnapshotDto?> GetPropertyAsync(
            Guid propertyId,
            CancellationToken cancellationToken);


        Task<CustomerStatusDto>
        GetCustomerStatusAsync(
        Guid authUserId,
        CancellationToken cancellationToken);
    }
}
