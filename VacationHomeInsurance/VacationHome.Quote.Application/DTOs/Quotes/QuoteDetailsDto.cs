using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Quotes
{
    public sealed class QuoteDetailsDto
    {
        public Guid Id { get; init; }


        public string QuoteNumber { get; init; } =
            string.Empty;


        public Guid CustomerId { get; init; }


        public Guid PropertyId { get; init; }


        public Guid ProductId { get; init; }


        public string ProductName { get; init; } =
            string.Empty;


        public string ProductDescription { get; init; } =
            string.Empty;


        public ProductType ProductType { get; init; }


        public QuoteStatus Status { get; init; }


        public string PropertyName { get; init; } =
            string.Empty;


        public string PropertyType { get; init; } =
            string.Empty;


        public string AddressSummary { get; init; } =
            string.Empty;


        public decimal PropertyValue { get; init; }


        public int ConstructionYear { get; init; }


        public string OccupancyFrequency { get; init; } =
            string.Empty;


        public int RiskScore { get; init; }


        public RiskLevel RiskLevel { get; init; }


        public string RiskExplanation { get; init; } =
            string.Empty;

        public string? PolicyConversionWarning { get; set; }


        public decimal BasePremium { get; init; }


        public decimal RiskAdjustmentAmount { get; init; }


        public decimal MandatoryCoverageCharges
        {
            get;
            init;
        }


        public decimal OptionalCoverageCharges
        {
            get;
            init;
        }


        public decimal FinalAnnualPremium { get; init; }


        public decimal EstimatedMonthlyEquivalent
        {
            get;
            init;
        }


        public string PaymentFrequency { get; init; } =
            string.Empty;


        public DateTime GeneratedAtUtc { get; init; }


        public DateTime ExpiresAtUtc { get; init; }


        public DateTime? PaymentDeadlineUtc { get; init; }


        public DateTime? CustomerAcceptedAtUtc
        {
            get;
            init;
        }


        public AdminDecision? AdminDecision { get; init; }


        public DateTime? AdminDecisionAtUtc
        {
            get;
            init;
        }


        public string? ApprovalRemarks { get; init; }


        public string? RejectionReason { get; init; }


        public bool PaymentEligible { get; init; }


        public IReadOnlyCollection<QuoteCoverageDto>
            Coverages
        {
            get;
            init;
        } = Array.Empty<QuoteCoverageDto>();
        public string RowVersion { get; init; } =
    string.Empty;
    }
}
