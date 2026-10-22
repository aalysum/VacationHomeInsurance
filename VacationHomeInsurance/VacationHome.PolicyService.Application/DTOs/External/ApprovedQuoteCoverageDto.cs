namespace VacationHome.PolicyService.Application.DTOs.External
{
    public sealed record ApprovedQuoteCoverageDto
    {
        public Guid CoverageId { get; init; }


        public string CoverageName { get; init; } = string.Empty;


        public string CoverageDescription { get; init; } = string.Empty;


        public bool IsMandatory { get; init; }


        public decimal CoverageCharge { get; init; }


        public decimal CoverageLimitPercentage { get; init; }


        public decimal CoverageLimitAmount { get; init; }
    }
}
