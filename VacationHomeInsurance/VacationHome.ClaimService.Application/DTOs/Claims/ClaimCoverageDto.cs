namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class ClaimCoverageDto
    {
        public Guid Id { get; set; }


        public Guid CoverageId { get; set; }


        public string CoverageName { get; set; } =
            string.Empty;


        public bool IsMandatoryCoverage { get; set; }


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


        public decimal RequestedAmount { get; set; }


        public decimal? ApprovedAmount { get; set; }
    }
}
