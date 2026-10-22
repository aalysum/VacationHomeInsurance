namespace VacationHome.ProductQuote.Application.DTOs.Pricing
{
    public sealed class PricingConfigurationDto
    {
        public Guid Id { get; init; }


        public decimal BaseRate { get; init; }


        public decimal LowRiskMultiplier { get; init; }


        public decimal MediumRiskMultiplier { get; init; }


        public decimal HighRiskMultiplier { get; init; }


        public bool IsActive { get; init; }


        public DateTime CreatedAtUtc { get; init; }


        public DateTime? UpdatedAtUtc { get; init; }


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
