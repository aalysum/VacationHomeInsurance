using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }


        public string PaymentNumber { get; set; } = string.Empty;


        public Guid QuoteId { get; set; }


        public Guid CustomerId { get; set; }


        public Guid? PolicyId { get; set; }


        public decimal Amount { get; set; }


        public PaymentStatus Status { get; set; }


        public int AttemptNumber { get; set; }


        public string CardholderName { get; set; } = string.Empty;


        public string Last4Digits { get; set; } = string.Empty;


        public DateTime ProcessedAtUtc { get; set; }


        public string? FailureReason { get; set; }


        public Policy? Policy { get; set; }


        public string RowVersion { get; set; } = string.Empty;
    }
}
