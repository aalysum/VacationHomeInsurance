using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Domain.Entities
{
    public class PolicyRenewal
    {
        public Guid Id { get; set; }


        public Guid ExistingPolicyId { get; set; }


        public Guid CustomerId { get; set; }


        public Guid? RenewalQuoteId { get; set; }


        public Guid? RenewedPolicyId { get; set; }


        public RenewalStatus Status { get; set; }


        public DateTime CreatedAtUtc { get; set; }


        public DateTime? CompletedAtUtc { get; set; }


        public string RowVersion { get; set; } = string.Empty;
    }
}
