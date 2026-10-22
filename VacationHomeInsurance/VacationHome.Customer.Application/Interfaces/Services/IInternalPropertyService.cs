using VacationHome.Customer.Application.DTOs.Internal;


namespace VacationHome.Customer.Application.Interfaces.Services
{
    public interface IInternalPropertyService
    {
        Task<InternalPropertySnapshotResponse> GetPropertySnapshotAsync(
            Guid propertyId,
            CancellationToken cancellationToken);
    }
}
