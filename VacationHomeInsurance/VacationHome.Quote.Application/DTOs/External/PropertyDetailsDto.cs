using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.External
{
    public sealed record PropertyDetailsDto(
        Guid PropertyId,
        Guid CustomerId,
        string PropertyName,
        PropertyType PropertyType,
        string AddressLine1,
        string? AddressLine2,
        string City,
        string State,
        string PostalCode,
        string Country,
        decimal PropertyValue,
        int ConstructionYear,
        OccupancyFrequency OccupancyFrequency,
        bool HasSecuritySystem,
        int PreviousClaimsCount,
        bool DisasterExposure,
        bool IsArchived);
}
