using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Domain.Entities
{
    public class QuoteApprovalHistory
    {
        public Guid Id { get; set; }


        public Guid QuoteId { get; set; }


        public Guid AdminId { get; set; }


        public AdminDecision Decision { get; set; }


        public string? Remarks { get; set; }


        public string? RejectionReason { get; set; }


        public string CorrelationId { get; set; } =
            string.Empty;


        public DateTime DecisionAtUtc { get; set; }


        public DateTime CreatedAtUtc { get; set; }


        public Quote Quote { get; set; } =
            default!;
    }
}
