using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Internal
{
    public sealed class ValidatedQuoteCoverageDto
    {
        public Guid CoverageId { get; init; }


        public Guid ProductCoverageId { get; init; }


        public string CoverageName { get; init; } =
            string.Empty;


        public string CoverageDescription { get; init; } =
            string.Empty;


        public CoverageType CoverageType { get; init; }


        public decimal CoverageCharge { get; init; }
        public bool IsMandatory { get; init; }


        public decimal CoverageLimitPercentage
        {
            get;
            init;
        }


        public decimal CoverageLimitAmount
        {
            get;
            init;
        }


        public int DisplayOrder { get; init; }
    }
}
