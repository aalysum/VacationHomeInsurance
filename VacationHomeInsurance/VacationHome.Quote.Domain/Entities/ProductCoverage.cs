using VacationHome.ProductQuote.Domain.Common;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Domain.Entities
{
    public class ProductCoverage : Entity
    {
        public Guid ProductId { get; set; }


        public Guid CoverageId { get; set; }


        public CoverageType CoverageType { get; set; }


        public decimal CoverageCharge { get; set; }


        public decimal CoverageLimitPercentage
        {
            get;
            set;
        }


        public int DisplayOrder { get; set; }


        public bool IsActive { get; set; }


        public Product Product { get; set; } =
            default!;


        public Coverage Coverage { get; set; } =
            default!;
    }
}
