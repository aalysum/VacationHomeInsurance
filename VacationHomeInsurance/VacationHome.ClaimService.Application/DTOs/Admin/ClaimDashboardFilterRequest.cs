using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.DTOs.Admin
{
    public sealed class ClaimDashboardFilterRequest
    {
        public ClaimStatus? Status { get; set; }


        public ClaimType? ClaimType { get; set; }


        public string? ClaimNumber { get; set; }


        public Guid? CustomerId { get; set; }


        public DateTime? FromDate { get; set; }


        public DateTime? ToDate { get; set; }


        public int PageNumber { get; set; } = 1;


        public int PageSize { get; set; } = 20;
    }
}
