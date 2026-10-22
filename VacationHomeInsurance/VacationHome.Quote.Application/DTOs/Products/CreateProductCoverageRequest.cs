namespace VacationHome.ProductQuote.Application.DTOs.Products
{
    public sealed class CreateProductCoverageRequest
    {
        public Guid ProductId { get; set; }


        public Guid CoverageId { get; set; }


        public bool IsMandatory { get; set; }


        public decimal CoverageCharge { get; set; }


        public decimal CoverageLimitPercentage { get; set; }
    }
}
