namespace VacationHome.Auth.Application.Common
{
    public class CustomerServiceOptions
    {
        public const string SectionName =
            "CustomerService";


        public string BaseUrl { get; set; }
            = string.Empty;


        public int TimeoutSeconds { get; set; }
            = 10;
    }
}
