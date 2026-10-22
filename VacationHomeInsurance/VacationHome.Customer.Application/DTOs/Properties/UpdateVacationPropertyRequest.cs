using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.DTOs.Properties
{
    public class UpdateVacationPropertyRequest
    {
        public string PropertyName { get; set; }
            = string.Empty;


        public PropertyType PropertyType { get; set; }


        public string AddressLine1 { get; set; }
            = string.Empty;


        public string? AddressLine2 { get; set; }


        public string City { get; set; }
            = string.Empty;


        public string State { get; set; }
            = string.Empty;


        public string PostalCode { get; set; }
            = string.Empty;


        public string Country { get; set; }
            = string.Empty;


        public decimal PropertyValue { get; set; }


        public int ConstructionYear { get; set; }


        public OccupancyFrequency OccupancyFrequency { get; set; }


        public bool HasSecuritySystem { get; set; }


        public int PreviousClaimsCount { get; set; }


        public bool DisasterExposure { get; set; }


        public string RowVersion { get; set; }
            = string.Empty;
    }
}
