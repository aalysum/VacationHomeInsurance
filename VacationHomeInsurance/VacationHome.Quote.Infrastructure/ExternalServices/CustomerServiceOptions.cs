namespace VacationHome.ProductQuote.Infrastructure.ExternalServices
{
    public sealed class CustomerServiceOptions
    {
        public const string SectionName = "CustomerService";


        public string BaseUrl { get; set; } =
            string.Empty;


        public string CustomerStatusPathTemplate
        {
            get;
            set;
        } = "/internal/customers/by-auth-user/{customerId}/status";


        public string PropertyPathTemplate
        {
            get;
            set;
        } = "/internal/properties/{propertyId}";


        public int TimeoutSeconds { get; set; } = 10;
    }
}
