using VacationHome.ProductQuote.Application.Exceptions;
using VacationHome.ProductQuote.Application
    .Interfaces.Services;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;
using VacationHome.ProductQuote.Domain.ValueObjects;


namespace VacationHome.ProductQuote.Application.Services
{
    public sealed class PremiumCalculator
        : IPremiumCalculator
    {
        public PremiumSnapshot Calculate(
            decimal propertyValue,
            RiskLevel riskLevel,
            decimal baseRate,
            decimal lowRiskMultiplier,
            decimal mediumRiskMultiplier,
            decimal highRiskMultiplier,
            decimal mandatoryCoverageCharges,
            decimal optionalCoverageCharges)
        {
            ValidateInputs(
                propertyValue,
                baseRate,
                lowRiskMultiplier,
                mediumRiskMultiplier,
                highRiskMultiplier,
                mandatoryCoverageCharges,
                optionalCoverageCharges);


            decimal riskMultiplier =
                riskLevel switch
                {
                    RiskLevel.Low =>
                        lowRiskMultiplier,


                    RiskLevel.Medium =>
                        mediumRiskMultiplier,


                    RiskLevel.High =>
                        highRiskMultiplier,


                    _ => throw new BadRequestException(
                        "INVALID_RISK_LEVEL",
                        "Unsupported risk level.")
                };


            decimal basePremium = Round(
                propertyValue * baseRate);


            decimal riskAdjustedPremium = Round(
                basePremium * riskMultiplier);


            decimal riskAdjustmentAmount = Round(
                riskAdjustedPremium - basePremium);


            decimal normalizedMandatoryCharges =
                Round(mandatoryCoverageCharges);


            decimal normalizedOptionalCharges =
                Round(optionalCoverageCharges);


            decimal finalAnnualPremium = Round(
                riskAdjustedPremium +
                normalizedMandatoryCharges +
                normalizedOptionalCharges);


            if (finalAnnualPremium <= 0)
            {
                throw new ConflictException(
                    "INVALID_FINAL_PREMIUM",
                    "Final annual premium must be greater than zero.");
            }


            decimal estimatedMonthlyEquivalent =
                CalculateMonthlyEquivalent(finalAnnualPremium);
            return new PremiumSnapshot(
                baseRate: baseRate,
                riskMultiplier: riskMultiplier,
                basePremium: basePremium,
                riskAdjustedPremium: riskAdjustedPremium,
                riskAdjustmentAmount: riskAdjustmentAmount,
                mandatoryCoverageCharges: normalizedMandatoryCharges,
                optionalCoverageCharges: normalizedOptionalCharges,
                finalAnnualPremium: finalAnnualPremium,
                estimatedMonthlyEquivalent: estimatedMonthlyEquivalent);


        }

        public static decimal CalculateMonthlyEquivalent(
            decimal annualPremium)
        {
            if (annualPremium <= 0 ||
                annualPremium > 9999999999999999.99m ||
                Round(annualPremium) != annualPremium)
            {
                throw new BadRequestException(
                    "INVALID_ANNUAL_PREMIUM",
                    "Annual premium must be positive, within the supported range, and contain no more than two decimal places.");
            }

            return Round(annualPremium / 12m);
        }


        private static void ValidateInputs(
            decimal propertyValue,
            decimal baseRate,
            decimal lowRiskMultiplier,
            decimal mediumRiskMultiplier,
            decimal highRiskMultiplier,
            decimal mandatoryCoverageCharges,
            decimal optionalCoverageCharges)
        {
            if (propertyValue <= 0)
            {
                throw new BadRequestException(
                    "INVALID_PROPERTY_VALUE",
                    "Property value must be greater than zero.");
            }


            if (baseRate < PricingAssumptions.MinimumBaseRate ||
                baseRate > PricingAssumptions.MaximumBaseRate)
            {
                throw new ConflictException(
                    "INVALID_BASE_RATE",
                    "Base rate must be between 0.0001 and 0.02 of the insured property value.");
            }


            if (lowRiskMultiplier <
                    PricingAssumptions.MinimumRiskMultiplier ||
                mediumRiskMultiplier <
                    PricingAssumptions.MinimumRiskMultiplier ||
                highRiskMultiplier <
                    PricingAssumptions.MinimumRiskMultiplier ||
                lowRiskMultiplier >
                    PricingAssumptions.MaximumRiskMultiplier ||
                mediumRiskMultiplier >
                    PricingAssumptions.MaximumRiskMultiplier ||
                highRiskMultiplier >
                    PricingAssumptions.MaximumRiskMultiplier ||
                lowRiskMultiplier > mediumRiskMultiplier ||
                mediumRiskMultiplier > highRiskMultiplier)
            {
                throw new ConflictException(
                    "INVALID_RISK_MULTIPLIER",
                    "Risk multipliers must be between 0.5 and 3 and increase from low to high risk.");
            }


            if (mandatoryCoverageCharges < 0)
            {
                throw new ConflictException(
                    "INVALID_MANDATORY_COVERAGE_CHARGES",
                    "Mandatory Coverage charges cannot be negative.");
            }


            if (optionalCoverageCharges < 0)
            {
                throw new ConflictException(
                    "INVALID_OPTIONAL_COVERAGE_CHARGES",
                    "Optional Coverage charges cannot be negative.");
            }
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
