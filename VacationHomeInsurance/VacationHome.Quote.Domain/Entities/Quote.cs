using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Domain.Entities
{
    public class Quote
    {
        #region Identity


        public Guid Id { get; set; }


        public string QuoteNumber { get; set; } =
            string.Empty;


        public Guid CustomerId { get; set; }


        public Guid PropertyId { get; set; }


        public Guid ProductId { get; set; }

        #endregion


        #region Quote Lifecycle


        public QuoteStatus Status { get; set; }


        public DateTime GeneratedAtUtc { get; set; }


        public DateTime ExpiresAtUtc { get; set; }


        public DateTime? ExpiredAtUtc { get; set; }


        public DateTime? PaymentDeadlineUtc { get; set; }


        #endregion


        #region Product Snapshot


        public ProductType ProductType { get; set; }


        public string ProductName { get; set; } =
            string.Empty;


        public string ProductDescription { get; set; } =
            string.Empty;


        #endregion


        #region Property Snapshot


        public string PropertyName { get; set; } =
            string.Empty;


        public string PropertyType { get; set; } =
            string.Empty;


        public string AddressLine1 { get; set; } =
            string.Empty;


        public string? AddressLine2 { get; set; }


        public string City { get; set; } =
            string.Empty;


        public string State { get; set; } =
            string.Empty;


        public string PostalCode { get; set; } =
            string.Empty;


        public string Country { get; set; } =
            string.Empty;


        public decimal PropertyValue { get; set; }


        public int ConstructionYear { get; set; }


        public int PropertyAge { get; set; }


        public string OccupancyFrequency { get; set; } =
            string.Empty;


        public bool HasSecuritySystem { get; set; }


        public int PreviousClaimsCount { get; set; }


        public bool HasDisasterExposure { get; set; }


        #endregion


        #region Risk Configuration Snapshot


        public Guid RiskConfigurationId { get; set; }


        public decimal MediumPropertyValueThreshold
        {
            get;
            set;
        }


        public decimal HighPropertyValueThreshold
        {
            get;
            set;
        }


        #endregion


        #region Risk Calculation Snapshot


        public int PropertyAgePoints { get; set; }


        public int PreviousClaimsPoints { get; set; }


        public int DisasterExposurePoints { get; set; }


        public int SecuritySystemPoints { get; set; }


        public int OccupancyPoints { get; set; }


        public int PropertyValuePoints { get; set; }


        public int RiskScore { get; set; }


        public RiskLevel RiskLevel { get; set; }


        public string RiskExplanation { get; set; } =
            string.Empty;


        #endregion


        #region Pricing Configuration Snapshot


        public Guid PricingConfigurationId { get; set; }


        public decimal BaseRate { get; set; }


        public decimal RiskMultiplier { get; set; }


        #endregion


        #region Premium Calculation Snapshot


        public decimal BasePremium { get; set; }


        public decimal RiskAdjustedPremium { get; set; }


        public decimal RiskAdjustmentAmount { get; set; }


        public decimal MandatoryCoverageCharges
        {
            get;
            set;
        }


        public decimal OptionalCoverageCharges
        {
            get;
            set;
        }


        public decimal FinalAnnualPremium { get; set; }


        public decimal EstimatedMonthlyEquivalent
        {
            get;
            set;
        }


        public string PaymentFrequency { get; set; } =
            string.Empty;


        #endregion


        #region Customer Acceptance


        public Guid? AcceptedByCustomerId { get; set; }


        public DateTime? CustomerAcceptedAtUtc { get; set; }


        public string? AcceptanceCorrelationId { get; set; }


        #endregion


        #region Admin Decision


        public Guid? DecidedByAdminId { get; set; }


        public AdminDecision? AdminDecision { get; set; }


        public DateTime? AdminDecisionAtUtc { get; set; }


        public string? ApprovalRemarks { get; set; }


        public string? RejectionReason { get; set; }


        public string? AdminDecisionCorrelationId
        {
            get;
            set;
        }


        #endregion


        #region Withdrawal


        public DateTime? WithdrawnAtUtc { get; set; }


        public string? WithdrawalReason { get; set; }


        public string? WithdrawalCorrelationId
        {
            get;
            set;
        }


        #endregion


        #region Invalidation


        public DateTime? InvalidatedAtUtc { get; set; }


        public string? InvalidationReason { get; set; }


        public string? InvalidationCorrelationId
        {
            get;
            set;
        }


        #endregion


        #region Policy Conversion


        public Guid? ConvertedPolicyId { get; set; }


        public DateTime? ConvertedAtUtc { get; set; }


        public string? ConversionCorrelationId
        {
            get;
            set;
        }


        #endregion


        #region Record Metadata


        public DateTime CreatedAtUtc { get; set; }


        public DateTime? UpdatedAtUtc { get; set; }


        #endregion


        #region Navigation Properties


        public Product Product { get; set; } =
            default!;


        public ICollection<QuoteCoverage> Coverages
        {
            get;
            set;
        } = new List<QuoteCoverage>();


        public ICollection<QuoteApprovalHistory> ApprovalHistory
        {
            get;
            set;
        } = new List<QuoteApprovalHistory>();


        #endregion


        #region Optimistic Concurrency


        public byte[] RowVersion { get; set; } =
            Array.Empty<byte>();


        #endregion
    }
}
