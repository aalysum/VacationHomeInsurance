namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class WithdrawClaimRequest
    {
        public string? Reason { get; set; }


        public string RowVersion { get; set; } = string.Empty;
    }
}
