namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class
        ApproveClaimCoverageRequest
    {
        public Guid CoverageId { get; set; }


        public decimal ApprovedAmount { get; set; }
    }
}
