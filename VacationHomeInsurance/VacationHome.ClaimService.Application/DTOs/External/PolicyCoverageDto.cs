namespace VacationHome.ClaimService.Application.DTOs.External
{
    public sealed class PolicyCoverageDto
    {
        public Guid CoverageId { get; set; }


        public string CoverageName { get; set; } =
            string.Empty;


        public bool IsMandatory { get; set; }


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
    }
}
