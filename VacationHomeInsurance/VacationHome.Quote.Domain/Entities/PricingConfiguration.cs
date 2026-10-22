using VacationHome.ProductQuote.Domain.Common;


namespace VacationHome.ProductQuote.Domain.Entities
{
    public class PricingConfiguration : Entity
    {
        public decimal BaseRate { get; set; }


        public decimal LowRiskMultiplier { get; set; }


        public decimal MediumRiskMultiplier { get; set; }


        public decimal HighRiskMultiplier { get; set; }


        public bool IsActive { get; set; }


        public DateTime CreatedAtUtc { get; set; }


        public DateTime? UpdatedAtUtc { get; set; }
    }
}
