using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;
using VacationHome.ProductQuote.Domain.ValueObjects;
using VacationHome.ProductQuote.Application.Exceptions;
using VacationHome.ProductQuote.Application.Interfaces.Services;




namespace VacationHome.ProductQuote.Application.Services
{
    public sealed class RiskCalculator : IRiskCalculator
    {
        private const int LowRiskMaximumScore = 39;
        private const int MediumRiskMaximumScore = 69;

        // Score weights and category cutoffs are illustrative project rules,
        // not actuarially validated measures.
        public RiskSnapshot Calculate(
            int constructionYear,
            decimal propertyValue,
            int previousClaimsCount,
            bool disasterExposure,
            bool securitySystem,
            string occupancyFrequency,
            RiskConfiguration riskConfiguration)
        {
            ArgumentNullException.ThrowIfNull(
                riskConfiguration);


            if (!riskConfiguration.IsActive)
            {
                throw new ConflictException(
                    "RISK_CONFIGURATION_INACTIVE",
                    "The Risk configuration is inactive.");
            }


            if (riskConfiguration.MediumPropertyValueThreshold <= 0 ||
                riskConfiguration.HighPropertyValueThreshold <=
                    riskConfiguration.MediumPropertyValueThreshold)
            {
                throw new ConflictException(
                    "INVALID_RISK_CONFIGURATION",
                    "Risk configuration thresholds are invalid.");
            }


            if (string.IsNullOrWhiteSpace(occupancyFrequency))
            {
                throw new BadRequestException(
                    "INVALID_OCCUPANCY_FREQUENCY",
                    "Occupancy frequency is required.");
            }


            DateTime utcNow = DateTime.UtcNow;


            if (constructionYear <= 0 ||
                constructionYear > utcNow.Year)
            {
                throw new BadRequestException(
                    "INVALID_CONSTRUCTION_YEAR",
                    "Construction year cannot be in the future.");
            }


            if (propertyValue <= 0)
            {
                throw new BadRequestException(
                    "INVALID_PROPERTY_VALUE",
                    "Property value must be greater than zero.");
            }


            if (previousClaimsCount < 0)
            {
                throw new BadRequestException(
                    "INVALID_PREVIOUS_CLAIMS_COUNT",
                    "Previous claims count cannot be negative.");
            }


            int propertyAge =
                utcNow.Year - constructionYear;


            int propertyAgePoints = propertyAge switch
            {
                <= 10 => RiskScoreAssumptions.PropertyAgeUpToTenYears,
                <= 20 => RiskScoreAssumptions.PropertyAgeUpToTwentyYears,
                <= 30 => RiskScoreAssumptions.PropertyAgeUpToThirtyYears,
                _ => RiskScoreAssumptions.PropertyAgeOverThirtyYears
            };


            int previousClaimsPoints =
                previousClaimsCount switch
                {
                    0 => RiskScoreAssumptions.NoPreviousClaims,
                    1 => RiskScoreAssumptions.OnePreviousClaim,
                    2 => RiskScoreAssumptions.TwoPreviousClaims,
                    _ => RiskScoreAssumptions.ThreeOrMorePreviousClaims
                };


            int disasterExposurePoints =
                disasterExposure
                    ? RiskScoreAssumptions.DisasterExposure
                    : 0;


            int securitySystemPoints =
                securitySystem
                    ? 0
                    : RiskScoreAssumptions.NoSecuritySystem;


            int occupancyPoints =
                occupancyFrequency
                    .Trim()
                    .ToUpperInvariant() switch
                {
                    "HIGH" => RiskScoreAssumptions.HighOccupancy,
                    "MEDIUM" => RiskScoreAssumptions.MediumOccupancy,
                    "LOW" => RiskScoreAssumptions.LowOccupancy,
                    _ => throw new BadRequestException(
                        "INVALID_OCCUPANCY_FREQUENCY",
                        "Occupancy frequency must be Low, Medium, or High.")
                };


            int propertyValuePoints;


            if (propertyValue <
                riskConfiguration.MediumPropertyValueThreshold)
            {
                propertyValuePoints =
                    RiskScoreAssumptions.BelowMediumPropertyThreshold;
            }
            else if (propertyValue <
                     riskConfiguration.HighPropertyValueThreshold)
            {
                propertyValuePoints =
                    RiskScoreAssumptions.MediumPropertyThreshold;
            }
            else
            {
                propertyValuePoints =
                    RiskScoreAssumptions.HighPropertyThreshold;
            }


            int totalRiskScore = Math.Min(
                100,
                propertyAgePoints +
                previousClaimsPoints +
                disasterExposurePoints +
                securitySystemPoints +
                occupancyPoints +
                propertyValuePoints);


            RiskLevel riskLevel = totalRiskScore switch
            {
                <= LowRiskMaximumScore => RiskLevel.Low,
                <= MediumRiskMaximumScore => RiskLevel.Medium,
                _ => RiskLevel.High
            };
            return new RiskSnapshot(
                propertyAge,
                propertyAgePoints,
                previousClaimsPoints,
                disasterExposurePoints,
                securitySystemPoints,
                occupancyPoints,
                propertyValuePoints,
                totalRiskScore,
                riskLevel,
                string.Join(
                    " ",
                    $"Property age contributed {propertyAgePoints} points.",
                    $"Previous claims contributed {previousClaimsPoints} points.",
                    $"Disaster exposure contributed {disasterExposurePoints} points.",
                    $"Security system contributed {securitySystemPoints} points.",
                    $"Occupancy frequency contributed {occupancyPoints} points.",
                    $"Property value contributed {propertyValuePoints} points."));


        }

        private static class RiskScoreAssumptions
        {
            public const int PropertyAgeUpToTenYears = 5;
            public const int PropertyAgeUpToTwentyYears = 10;
            public const int PropertyAgeUpToThirtyYears = 15;
            public const int PropertyAgeOverThirtyYears = 20;
            public const int NoPreviousClaims = 0;
            public const int OnePreviousClaim = 10;
            public const int TwoPreviousClaims = 20;
            public const int ThreeOrMorePreviousClaims = 30;
            public const int DisasterExposure = 25;
            public const int NoSecuritySystem = 15;
            public const int HighOccupancy = 0;
            public const int MediumOccupancy = 10;
            public const int LowOccupancy = 20;
            public const int BelowMediumPropertyThreshold = 0;
            public const int MediumPropertyThreshold = 5;
            public const int HighPropertyThreshold = 10;
        }
    }
}
