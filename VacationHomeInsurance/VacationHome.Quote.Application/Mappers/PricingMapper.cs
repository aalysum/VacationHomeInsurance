using VacationHome.ProductQuote.Application.DTOs.Pricing;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Mappers
{
    public static class PricingMapper
    {
        public static PricingConfigurationDto ToDto(
            this PricingConfiguration pricing)
        {
            ArgumentNullException.ThrowIfNull(
                pricing);


            return new PricingConfigurationDto
            {
                Id =
                    pricing.Id,


                BaseRate =
                    pricing.BaseRate,


                LowRiskMultiplier =
                    pricing.LowRiskMultiplier,


                MediumRiskMultiplier =
                    pricing.MediumRiskMultiplier,


                HighRiskMultiplier =
                    pricing.HighRiskMultiplier,


                IsActive =
                    pricing.IsActive,


                CreatedAtUtc =
                    pricing.CreatedAtUtc,


                UpdatedAtUtc =
                    pricing.UpdatedAtUtc,


                RowVersion =
                    Convert.ToBase64String(
                        pricing.RowVersion)
            };
        }
    }
}
