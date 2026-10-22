using VacationHome.ProductQuote.Domain.Enums;
using VacationHome.ProductQuote.Domain.ValueObjects;


namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface IPremiumCalculator
    {
        PremiumSnapshot Calculate(
            decimal propertyValue,
            RiskLevel riskLevel,
            decimal baseRate,
            decimal lowRiskMultiplier,
            decimal mediumRiskMultiplier,
            decimal highRiskMultiplier,
            decimal mandatoryCoverageCharges,
            decimal optionalCoverageCharges);
    }
}
