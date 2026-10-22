namespace VacationHome.ClaimService.Application.DTOs.Admin
{
    public sealed class AssignClaimsManagerRequest
    {
        public string ClaimsManagerId { get; set; } =
            string.Empty;


        public string? Reason { get; set; }


        public string RowVersion { get; set; } =
            string.Empty;
    }
}
