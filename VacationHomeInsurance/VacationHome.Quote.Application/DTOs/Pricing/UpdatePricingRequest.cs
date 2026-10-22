namespace VacationHome.ProductQuote.Application.DTOs.Pricing
{
    public sealed class UpdatePricingRequest
    {
        public decimal BaseRate { get; init; }


        public decimal LowRiskMultiplier { get; init; }


        public decimal MediumRiskMultiplier { get; init; }


        public decimal HighRiskMultiplier { get; init; }


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
