using VacationHome.ClaimService.Application.DTOs.Claims;


namespace VacationHome.ClaimService.Application.DTOs.Admin
{
    public sealed class ClaimDashboardDto
    {
        public long TotalClaims { get; set; }


        public long SubmittedClaims { get; set; }


        public long UnderReviewClaims { get; set; }


        public long InformationRequestedClaims { get; set; }


        public long ApprovedClaims { get; set; }


        public long RejectedClaims { get; set; }


        public long PaidClaims { get; set; }


        public long ClosedClaims { get; set; }


        public decimal TotalClaimedAmount { get; set; }


        public decimal TotalApprovedAmount { get; set; }


        public IReadOnlyCollection<ClaimSummaryDto> RecentClaims { get; set; }
            = Array.Empty<ClaimSummaryDto>();
    }
}
