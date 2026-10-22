using VacationHome.ProductQuote.Application.DTOs.Common;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Admin
{
    public sealed class AdminQuoteSearchRequest : PageRequest
    {
        public Guid? CustomerId { get; init; }


        public Guid? PropertyId { get; init; }


        public Guid? ProductId { get; init; }


        public QuoteStatus? Status { get; init; }


        public DateTime? GeneratedFromUtc { get; init; }


        public DateTime? GeneratedToUtc { get; init; }


        public DateTime? DecisionFromUtc { get; init; }


        public DateTime? DecisionToUtc { get; init; }
    }
}
