namespace VacationHome.ProductQuote.Domain.ValueObjects
{
    public sealed record PremiumSnapshot
    {
        private PremiumSnapshot()
        {
        }


        public PremiumSnapshot(
            decimal baseRate,
            decimal riskMultiplier,
            decimal basePremium,
            decimal riskAdjustedPremium,
            decimal riskAdjustmentAmount,
            decimal mandatoryCoverageCharges,
            decimal optionalCoverageCharges,
            decimal finalAnnualPremium,
            decimal estimatedMonthlyEquivalent)
        {
            BaseRate = baseRate;
            RiskMultiplier = riskMultiplier;
            BasePremium = Round(basePremium);
            RiskAdjustedPremium = Round(riskAdjustedPremium);
            RiskAdjustmentAmount = Round(riskAdjustmentAmount);
            MandatoryCoverageCharges =
                Round(mandatoryCoverageCharges);
            OptionalCoverageCharges =
                Round(optionalCoverageCharges);
            FinalAnnualPremium =
                Round(finalAnnualPremium);
            EstimatedMonthlyEquivalent =
                Round(estimatedMonthlyEquivalent);
        }


        public decimal BaseRate { get; private set; }


        public decimal RiskMultiplier { get; private set; }


        public decimal BasePremium { get; private set; }


        public decimal RiskAdjustedPremium { get; private set; }


        public decimal RiskAdjustmentAmount { get; private set; }


        public decimal MandatoryCoverageCharges
        {
            get;
            private set;
        }


        public decimal OptionalCoverageCharges
        {
            get;
            private set;
        }


        public decimal FinalAnnualPremium
        {
            get;
            private set;
        }


        public decimal EstimatedMonthlyEquivalent
        {
            get;
            private set;
        }


        private static decimal Round(decimal value)
        {
            return Math.Round(
                value,
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}
