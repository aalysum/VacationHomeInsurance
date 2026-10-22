using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Internal
{
    public sealed class ValidatedQuoteDto
    {
        public Guid QuoteId { get; init; }


        public string QuoteNumber { get; init; } =
            string.Empty;


        public Guid CustomerId { get; init; }


        public Guid PropertyId { get; init; }


        public Guid ProductId { get; init; }
        public bool IsEligible { get; init; }


        public string? FailureCode { get; init; }


        public string? FailureMessage { get; init; }


        public QuoteStatus Status { get; init; }


        public DateTime GeneratedAtUtc { get; init; }


        public DateTime ExpiresAtUtc { get; init; }


        public DateTime? PaymentDeadlineUtc { get; init; }


        public Guid? AcceptedByCustomerId { get; init; }


        public DateTime? CustomerAcceptedAtUtc { get; init; }


        public Guid? DecidedByAdminId { get; init; }


        public AdminDecision? AdminDecision { get; init; }


        public DateTime? AdminDecisionAtUtc { get; init; }


        public string? ApprovalRemarks { get; init; }


        public ProductType ProductType { get; init; }


        public string ProductName { get; init; } =
            string.Empty;


        public string ProductDescription { get; init; } =
            string.Empty;


        public string PropertyName { get; init; } =
            string.Empty;


        public string PropertyType { get; init; } =
            string.Empty;


        public string AddressLine1 { get; init; } =
            string.Empty;


        public string? AddressLine2 { get; init; }


        public string City { get; init; } =
            string.Empty;


        public string State { get; init; } =
            string.Empty;


        public string PostalCode { get; init; } =
            string.Empty;


        public string Country { get; init; } =
            string.Empty;


        public decimal PropertyValue { get; init; }


        public int ConstructionYear { get; init; }


        public int PropertyAge { get; init; }


        public string OccupancyFrequency { get; init; } =
            string.Empty;


        public bool HasSecuritySystem { get; init; }


        public int PreviousClaimsCount { get; init; }


        public bool HasDisasterExposure { get; init; }


        public int RiskScore { get; init; }


        public RiskLevel RiskLevel { get; init; }


        public string RiskExplanation { get; init; } =
            string.Empty;


        public Guid PricingConfigurationId { get; init; }


        public decimal BaseRate { get; init; }


        public decimal RiskMultiplier { get; init; }


        public decimal BasePremium { get; init; }


        public decimal RiskAdjustedPremium { get; init; }


        public decimal RiskAdjustmentAmount { get; init; }


        public decimal MandatoryCoverageCharges { get; init; }


        public decimal OptionalCoverageCharges { get; init; }


        public decimal FinalAnnualPremium { get; init; }


        public decimal EstimatedMonthlyEquivalent { get; init; }


        public string PaymentFrequency { get; init; } =
            string.Empty;


        public IReadOnlyCollection<ValidatedQuoteCoverageDto>
            Coverages
        { get; init; } =
                Array.Empty<ValidatedQuoteCoverageDto>();


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
