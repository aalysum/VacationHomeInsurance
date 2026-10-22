namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class MarkClaimPaidRequest
    {
        public string PaymentReference { get; set; } = string.Empty;


        public string RowVersion { get; set; } = string.Empty;
    }
}
