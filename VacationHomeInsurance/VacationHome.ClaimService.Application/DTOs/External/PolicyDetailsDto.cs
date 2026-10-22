using VacationHome.ClaimService.Domain.Enums;
using System.Text.Json.Serialization;
using VacationHome.ClaimService.Application.DTOs.Documents;


namespace VacationHome.ClaimService.Application.DTOs.External
{
    public sealed class PolicyDetailsDto
    {
        public Guid Id { get; set; }


        public string PolicyNumber { get; set; } =
            string.Empty;


        public Guid QuoteId { get; set; }


        public Guid CustomerId { get; set; }


        public Guid PropertyId { get; set; }

        public string PropertyName { get; set; } = string.Empty;


        public string PropertyAddressLine1 { get; set; } = string.Empty;


        public string? PropertyAddressLine2 { get; set; }


        public string PropertyCity { get; set; } = string.Empty;


        public string PropertyState { get; set; } = string.Empty;


        public string PropertyPostalCode { get; set; } = string.Empty;


        public string PropertyCountry { get; set; } = string.Empty;


        public string PropertyType { get; set; } = string.Empty;

        public decimal PropertyValue { get; set; }


        public int ConstructionYear { get; set; }


        public string OccupancyFrequency { get; set; } = string.Empty;


        public bool HasSecuritySystem { get; set; }


        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProductType ProductType { get; set; }


        public string ProductName { get; set; } =
            string.Empty;


        public decimal AnnualPremium { get; set; }


        public DateTime EffectiveDateUtc { get; set; }


        public DateTime ExpiryDateUtc { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PolicyStatus Status { get; set; }


        public IReadOnlyCollection<PolicyCoverageDto>
            Coverages
        {
            get;
            set;
        } = Array.Empty<PolicyCoverageDto>();

        public IReadOnlyCollection<PolicyDocumentMetadataDto>
            Documents
        {
            get;
            set;
        } = Array.Empty<PolicyDocumentMetadataDto>();


        public bool IsActive =>
            Status == PolicyStatus.Active;
    }
}
