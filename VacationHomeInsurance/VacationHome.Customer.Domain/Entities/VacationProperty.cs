using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Domain.Entities
{
    public class VacationProperty
    {
        public Guid Id { get; set; }


        public Guid CustomerId { get; set; }


        public string PropertyName { get; set; } = string.Empty;


        public PropertyType PropertyType { get; set; }


        public string AddressLine1 { get; set; } = string.Empty;


        public string? AddressLine2 { get; set; }


        public string City { get; set; } = string.Empty;


        public string State { get; set; } = string.Empty;


        public string PostalCode { get; set; } = string.Empty;


        public string Country { get; set; } = string.Empty;


        public decimal PropertyValue { get; set; }


        public int ConstructionYear { get; set; }


        public OccupancyFrequency OccupancyFrequency { get; set; }


        public bool HasSecuritySystem { get; set; }


        public int PreviousClaimsCount { get; set; }


        public bool DisasterExposure { get; set; }


        public bool IsArchived { get; set; }


        public DateTime? ArchivedAtUtc { get; set; }


        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;


        public DateTime? UpdatedAtUtc { get; set; }


        public byte[] RowVersion { get; set; } = null!;


        public Customers Customer { get; set; } = null!;
    }
}
