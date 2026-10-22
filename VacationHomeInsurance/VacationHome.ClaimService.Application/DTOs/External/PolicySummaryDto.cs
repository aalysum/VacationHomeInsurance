namespace VacationHome.ClaimService.Application.DTOs.External
{
    public sealed class PolicySummaryDto
    {
        public Guid PolicyId { get; set; }


        public string PolicyNumber { get; set; } = string.Empty;


        public Guid CustomerId { get; set; }


        public Guid PropertyId { get; set; }


        public DateTime EffectiveDate { get; set; }


        public DateTime ExpirationDate { get; set; }


        public bool IsActive { get; set; }


        public decimal CoverageLimit { get; set; }


        public decimal Deductible { get; set; }


        public string ProductCode { get; set; } = string.Empty;
    }
}
