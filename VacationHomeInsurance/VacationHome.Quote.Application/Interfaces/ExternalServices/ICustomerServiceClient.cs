using VacationHome.ProductQuote.Application.DTOs.External;


namespace VacationHome.ProductQuote.Application.Interfaces.External
{
    public interface ICustomerServiceClient
    {
        Task<CustomerStatusDto> GetCustomerStatusAsync(
            Guid customerId,
            CancellationToken cancellationToken);


        Task<PropertyDetailsDto?> GetPropertyAsync(
            Guid propertyId,
            CancellationToken cancellationToken);
    }
}
