namespace VacationHome.ProductQuote.Domain.ValueObjects
{
    public sealed record QuoteConfigurationSnapshot(
        Guid PricingConfigurationId,
        DateTime ConfigurationCapturedAtUtc);
}
