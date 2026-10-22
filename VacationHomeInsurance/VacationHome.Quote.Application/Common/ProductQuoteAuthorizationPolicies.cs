namespace VacationHome.ProductQuote.Application.Common
{
    public static class ProductQuoteAuthorizationPolicies
    {
        public const string CustomerOnly =
            "ProductQuote.CustomerOnly";


        public const string AdminOnly =
            "ProductQuote.AdminOnly";


        public const string InternalServiceOnly =
            "ProductQuote.InternalServiceOnly";
    }
}
