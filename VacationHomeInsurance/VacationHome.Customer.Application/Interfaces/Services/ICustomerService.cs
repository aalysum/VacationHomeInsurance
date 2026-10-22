using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Application.DTOs.Customers;
using VacationHome.Customer.Application.DTOs.Properties;


namespace VacationHome.Customer.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<CustomerProfileResponse> GetProfileAsync(
            CancellationToken cancellationToken);


        Task UpdateProfileAsync(
            UpdateCustomerProfileRequest request,
            CancellationToken cancellationToken);


        Task<Guid> CreatePropertyAsync(
            CreateVacationPropertyRequest request,
            CancellationToken cancellationToken);


        Task<PagedResult<VacationPropertyResponse>>
            GetPropertiesAsync(
                PropertyQueryParameters parameters,
                CancellationToken cancellationToken);


        Task<VacationPropertyResponse> GetPropertyAsync(
            Guid propertyId,
            CancellationToken cancellationToken);


        Task UpdatePropertyAsync(
            Guid propertyId,
            UpdateVacationPropertyRequest request,
            CancellationToken cancellationToken);


        Task ArchivePropertyAsync(
            Guid propertyId,
            ArchiveVacationPropertyRequest request,
            CancellationToken cancellationToken);
    }
}
