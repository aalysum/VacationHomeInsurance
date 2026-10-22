using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Products
{
    public sealed class UpdateProductCoverageRequest
    {
        public CoverageType CoverageType { get; init; }


        public decimal CoverageCharge { get; init; }


        public decimal CoverageLimitPercentage
        {
            get;
            init;
        }


        public int DisplayOrder { get; init; }


        public bool IsActive { get; init; }


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
