namespace VacationHome.ProductQuote.Application.DTOs.Products
{
    public sealed class UpdateCoverageLimitRequest
    {
        public decimal CoverageLimitPercentage
        {
            get;
            init;
        }


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
