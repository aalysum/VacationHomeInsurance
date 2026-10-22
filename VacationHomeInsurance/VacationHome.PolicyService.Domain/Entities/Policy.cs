using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Domain.Entities
{
    public class Policy
    {
        public Guid Id { get; set; }


        public string PolicyNumber { get; set; } = string.Empty;


        public Guid QuoteId { get; set; }


        public Guid CustomerId { get; set; }


        public Guid PropertyId { get; set; }
        public string PropertyName { get; set; }
            = string.Empty;


        public string CustomerFirstName { get; set; } = string.Empty;


        public string CustomerLastName { get; set; } = string.Empty;


        public string CustomerPhoneNumber { get; set; } = string.Empty;


        public string PropertyAddressLine1 { get; set; } = string.Empty;


        public string? PropertyAddressLine2 { get; set; }


        public string PropertyCity { get; set; } = string.Empty;


        public string PropertyState { get; set; } = string.Empty;


        public string PropertyPostalCode { get; set; } = string.Empty;


        public string PropertyCountry { get; set; } = string.Empty;


        public string PropertyType { get; set; } = string.Empty;


        public decimal PropertyValue { get; set; }


        public decimal BaseRate { get; set; }


        public decimal RiskMultiplier { get; set; }


        public decimal BasePremium { get; set; }


        public decimal RiskAdjustedPremium { get; set; }


        public decimal RiskAdjustmentAmount { get; set; }


        public decimal MandatoryCoverageCharges { get; set; }


        public decimal OptionalCoverageCharges { get; set; }


        public string PaymentFrequency { get; set; } = string.Empty;


        public int ConstructionYear { get; set; }


        public int PropertyAge { get; set; }


        public string OccupancyFrequency { get; set; } = string.Empty;


        public bool HasSecuritySystem { get; set; }


        public int PreviousClaimsCount { get; set; }


        public bool DisasterExposure { get; set; }


        public int RiskScore { get; set; }


        public string RiskLevel { get; set; } = string.Empty;


        public string RiskExplanation { get; set; } = string.Empty;


        public Guid PaymentId { get; set; }


        public Guid? PreviousPolicyId { get; set; }


        public string ProductName { get; set; } = string.Empty;


        public decimal AnnualPremium { get; set; }


        public DateTime EffectiveDateUtc { get; set; }


        public DateTime ExpiryDateUtc { get; set; }


        public PolicyStatus Status { get; set; }


        public string? CancellationReason { get; set; }


        public DateTime? CancelledAtUtc { get; set; }


        public DateTime CreatedAtUtc { get; set; }


        public DateTime UpdatedAtUtc { get; set; }
        public ProductType ProductType { get; set; }


        public ICollection<PolicyCoverage> Coverages { get; set; }
            = new List<PolicyCoverage>();


        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();


        public ICollection<PolicyDocument> Documents { get; set; }
            = new List<PolicyDocument>();


        public string RowVersion { get; set; } = string.Empty;
    }
}
