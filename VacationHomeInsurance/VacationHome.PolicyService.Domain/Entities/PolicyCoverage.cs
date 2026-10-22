namespace VacationHome.PolicyService.Domain.Entities
{
    public class PolicyCoverage
    {
        public Guid Id { get; set; }


        public Guid PolicyId { get; set; }


        public Guid CoverageId { get; set; }


        public string CoverageName { get; set; } = string.Empty;


        public string CoverageDescription { get; set; } = string.Empty;


        public bool IsMandatory { get; set; }


        public decimal CoverageCharge { get; set; }


        public decimal CoverageLimitPercentage { get; set; }


        public decimal CoverageLimitAmount { get; set; }


        public DateTime CreatedAtUtc { get; set; }


        public Policy Policy { get; set; } = null!;
    }
}
