using System.Text.Json.Serialization;
using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.DTOs.External
{
    public sealed record ApprovedQuoteDto
    {
        public Guid QuoteId { get; init; }


        public string QuoteNumber { get; init; } =
            string.Empty;


        public Guid CustomerId { get; init; }


        public Guid PropertyId { get; init; }


        public Guid ProductId { get; init; }


        public string ProductName { get; init; } =
            string.Empty;
        public string PropertyName { get; init; }
            = string.Empty;


        public string PropertyType { get; init; } = string.Empty;


        public string AddressLine1 { get; init; } = string.Empty;


        public string? AddressLine2 { get; init; }


        public string City { get; init; } = string.Empty;


        public string State { get; init; } = string.Empty;


        public string PostalCode { get; init; } = string.Empty;


        public string Country { get; init; } = string.Empty;


        public decimal PropertyValue { get; init; }


        public int ConstructionYear { get; init; }


        public int PropertyAge { get; init; }


        public string OccupancyFrequency { get; init; } = string.Empty;


        public bool HasSecuritySystem { get; init; }


        public int PreviousClaimsCount { get; init; }


        public bool HasDisasterExposure { get; init; }


        public int RiskScore { get; init; }


        public int RiskLevel { get; init; }


        public string RiskExplanation { get; init; } = string.Empty;


        public ProductType ProductType { get; init; }


        public decimal FinalAnnualPremium { get; init; }


        public decimal BaseRate { get; init; }


        public decimal RiskMultiplier { get; init; }


        public decimal BasePremium { get; init; }


        public decimal RiskAdjustedPremium { get; init; }


        public decimal RiskAdjustmentAmount { get; init; }


        public decimal MandatoryCoverageCharges { get; init; }


        public decimal OptionalCoverageCharges { get; init; }


        public string PaymentFrequency { get; init; } = string.Empty;


        [JsonPropertyName("isEligible")]
        public bool IsEligibleForPayment { get; init; }


        [JsonPropertyName("failureCode")]
        public string? IneligibleCode { get; init; }


        [JsonPropertyName("failureMessage")]
        public string? IneligibleReason { get; init; }


        [JsonPropertyName("expiresAtUtc")]
        public DateTime QuoteExpiryDateUtc { get; init; }


        public DateTime? PaymentDeadlineUtc { get; init; }


        public string RowVersion { get; init; } =
            string.Empty;


        public IReadOnlyCollection<ApprovedQuoteCoverageDto>
            Coverages
        {
            get;
            init;
        } = Array.Empty<ApprovedQuoteCoverageDto>();
    }
}
