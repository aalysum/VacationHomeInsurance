namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class RejectClaimRequest
    {
        public string Reason { get; set; } = string.Empty;


        public string? Notes { get; set; }


        public string RowVersion { get; set; } = string.Empty;
    }
}
