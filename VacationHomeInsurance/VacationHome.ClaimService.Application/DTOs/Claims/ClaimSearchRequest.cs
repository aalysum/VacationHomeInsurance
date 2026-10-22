using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class ClaimSearchRequest
    {
        public ClaimStatus? Status { get; set; }


        public string? ClaimNumber { get; set; }


        public int PageNumber { get; set; } = 1;


        public int PageSize { get; set; } = 20;
    }
}
