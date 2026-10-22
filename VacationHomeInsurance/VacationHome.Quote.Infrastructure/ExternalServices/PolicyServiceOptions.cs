namespace VacationHome.ProductQuote.Infrastructure.ExternalServices
{
    public sealed class PolicyServiceOptions
    {
        public const string SectionName = "PolicyService";


        public string BaseUrl { get; set; } =
            string.Empty;


        public string CheckOverlapPath { get; set; } =
            "/internal/policies/check-overlap";


        public int TimeoutSeconds { get; set; } = 10;
    }
}
