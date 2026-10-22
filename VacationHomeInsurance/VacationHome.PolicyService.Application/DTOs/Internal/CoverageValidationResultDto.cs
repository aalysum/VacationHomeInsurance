namespace VacationHome.PolicyService.Application.DTOs.Internal
{
    public sealed class CoverageValidationResultDto
    {
        public bool IsCovered { get; set; }


        public string? Reason { get; set; }


        public decimal? MaximumClaimAmount { get; set; }
    }
}
