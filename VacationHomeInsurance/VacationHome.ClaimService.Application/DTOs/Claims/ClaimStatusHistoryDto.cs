using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class ClaimStatusHistoryDto
    {
        public ClaimStatus PreviousStatus { get; set; }


        public ClaimStatus NewStatus { get; set; }


        public string Comments { get; set; } = string.Empty;


        public string ChangedBy { get; set; } = string.Empty;


        public DateTime ChangedAtUtc { get; set; }
    }
}
