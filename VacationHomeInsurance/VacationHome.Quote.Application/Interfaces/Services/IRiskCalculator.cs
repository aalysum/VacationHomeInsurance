using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.ValueObjects;


namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface IRiskCalculator
    {
        RiskSnapshot Calculate(
            int constructionYear,
            decimal propertyValue,
            int previousClaimsCount,
            bool disasterExposure,
            bool securitySystem,
            string occupancyFrequency,
            RiskConfiguration riskConfiguration);
    }
}
