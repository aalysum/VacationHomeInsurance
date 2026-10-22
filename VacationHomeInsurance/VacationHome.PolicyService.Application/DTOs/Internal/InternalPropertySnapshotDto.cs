using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.DTOs.Internal
{
    public sealed class InternalPropertySnapshotDto
    {
        public Guid PropertyId { get; set; }


        public Guid CustomerId { get; set; }


        public string AuthUserId { get; set; }
            = string.Empty;


        public string PropertyName { get; set; }
            = string.Empty;


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


        public int PropertyAge { get; set; }


        public PropertyType PropertyType { get; set; }


        public OccupancyFrequency OccupancyFrequency { get; set; }


        public bool HasSecuritySystem { get; set; }


        public int PreviousClaimsCount { get; set; }


        public bool DisasterExposure { get; set; }


        public bool IsArchived { get; set; }


        public string SourceVersion { get; set; }
            = string.Empty;


        public DateTime? UpdatedAtUtc { get; set; }
    }
}
