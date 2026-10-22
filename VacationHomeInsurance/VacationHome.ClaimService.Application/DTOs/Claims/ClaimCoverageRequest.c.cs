namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class ClaimCoverageRequest
    {
        public Guid CoverageId { get; set; }


        public decimal RequestedAmount { get; set; }
    }
}
