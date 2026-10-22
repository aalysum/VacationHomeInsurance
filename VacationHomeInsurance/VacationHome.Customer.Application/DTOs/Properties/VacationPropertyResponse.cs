using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.DTOs.Properties
{
    public class VacationPropertyResponse
    {
        public Guid PropertyId { get; init; }


        public string PropertyName { get; init; }
            = string.Empty;


        public PropertyType PropertyType { get; init; }


        public string AddressLine1 { get; init; }
            = string.Empty;


        public string? AddressLine2 { get; init; }


        public string City { get; init; }
            = string.Empty;


        public string State { get; init; }
            = string.Empty;


        public string PostalCode { get; init; }
            = string.Empty;


        public string Country { get; init; }
            = string.Empty;


        public decimal PropertyValue { get; init; }


        public int ConstructionYear { get; init; }


        public int PropertyAge { get; init; }


        public OccupancyFrequency OccupancyFrequency { get; init; }


        public bool HasSecuritySystem { get; init; }


        public int PreviousClaimsCount { get; init; }


        public bool DisasterExposure { get; init; }


        public bool IsArchived { get; init; }


        public DateTime? ArchivedAtUtc { get; init; }


        public DateTime CreatedAtUtc { get; init; }


        public DateTime? UpdatedAtUtc { get; init; }


        public string RowVersion { get; init; }
            = string.Empty;
    }
}
