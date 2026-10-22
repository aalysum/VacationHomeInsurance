namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class ApproveClaimRequest
    {
        public IReadOnlyCollection<
            ApproveClaimCoverageRequest>
            Coverages
        {
            get;
            set;
        } = Array.Empty<
            ApproveClaimCoverageRequest>();


        public string? Notes { get; set; }


        public string RowVersion { get; set; } =
            string.Empty;
    }
}
