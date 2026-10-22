namespace VacationHome.ClaimService.Application.DTOs.External
{
    public sealed class CoverageValidationResultDto
    {
        public bool IsCovered { get; set; }


        public string? Reason { get; set; }


        public decimal CoverageLimit { get; set; }


        public decimal Deductible { get; set; }


        public DateTime CoverageStartDate { get; set; }


        public DateTime CoverageEndDate { get; set; }
    }
}
