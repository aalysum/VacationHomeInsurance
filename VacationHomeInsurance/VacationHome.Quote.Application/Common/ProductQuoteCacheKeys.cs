namespace VacationHome.ProductQuote.Application.Common
{
    public static class ProductQuoteCacheKeys
    {
        public const string ActiveProducts =
            "productquote:products:active";


        public const string AllProducts =
            "productquote:products:all";


        public const string Coverages =
            "productquote:coverages:all";


        public const string ActivePricing =
            "productquote:pricing:active";


        public const string ActiveRiskConfiguration =
            "productquote:risk:active";


        public static string Product(Guid productId)
        {
            return $"productquote:product:{productId}";
        }


        public static string ProductCoverages(Guid productId)
        {
            return $"productquote:product:{productId}:coverages";
        }
    }
}
