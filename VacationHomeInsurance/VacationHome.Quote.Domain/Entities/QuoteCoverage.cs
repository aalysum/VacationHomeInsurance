using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Domain.Entities
{
    public class QuoteCoverage
    {
        public Guid Id { get; set; }


        public Guid QuoteId { get; set; }


        public Guid CoverageId { get; set; }


        public Guid ProductCoverageId { get; set; }


        public string CoverageName { get; set; } =
            string.Empty;


        public string CoverageDescription { get; set; } =
            string.Empty;


        public CoverageType CoverageType { get; set; }


        public decimal CoverageCharge { get; set; }


        public decimal CoverageLimitPercentage
        {
            get;
            set;
        }


        public decimal CoverageLimitAmount
        {
            get;
            set;
        }


        public int DisplayOrder { get; set; }


        public DateTime CreatedAtUtc { get; set; }


        public Quote Quote { get; set; } =
            default!;
    }
}
