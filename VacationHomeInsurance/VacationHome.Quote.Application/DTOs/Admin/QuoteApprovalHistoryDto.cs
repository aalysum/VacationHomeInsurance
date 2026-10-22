using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Admin
{
    public sealed class QuoteApprovalHistoryDto
    {
        public Guid Id { get; init; }


        public Guid QuoteId { get; init; }


        public Guid AdminId { get; init; }


        public AdminDecision Decision { get; init; }


        public string? Remarks { get; init; }


        public string? RejectionReason { get; init; }


        public string CorrelationId { get; init; } =
            string.Empty;


        public DateTime DecisionAtUtc { get; init; }


        public DateTime CreatedAtUtc { get; init; }
    }
}
