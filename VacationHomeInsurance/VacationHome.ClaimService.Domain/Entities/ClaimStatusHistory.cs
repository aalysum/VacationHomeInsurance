using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Domain.Entities
{
    public class ClaimStatusHistory
    {
        public Guid Id { get; set; }


        public Guid ClaimId { get; set; }


        public ClaimStatus PreviousStatus { get; set; }


        public ClaimStatus NewStatus { get; set; }


        public string Comments { get; set; } = string.Empty;


        public string ChangedBy { get; set; } = string.Empty;


        public DateTime ChangedAtUtc { get; set; }


        public Claim? Claim { get; set; }
    }
}
