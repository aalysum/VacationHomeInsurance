namespace VacationHome.ProductQuote.Application.Configuration
{
    public sealed class ProductQuoteOptions
    {
        public const string SectionName =
            "ProductQuote";


        public int QuoteValidityDays { get; set; } = 30;


        public int ApprovedPaymentPeriodDays { get; set; } =
            30;


        public int CacheDurationMinutes { get; set; } = 30;




    }
}
