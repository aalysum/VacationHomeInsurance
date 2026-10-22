namespace VacationHome.ProductQuote.Application.Services
{
    // Bounds for illustrative project inputs, not actuarially validated market rates.
    public static class PricingAssumptions
    {
        public const decimal MinimumBaseRate = 0.0001m;
        public const decimal MaximumBaseRate = 0.02m;
        public const decimal MinimumRiskMultiplier = 0.5m;
        public const decimal MaximumRiskMultiplier = 3m;
    }
}
