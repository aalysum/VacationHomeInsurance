using VacationHome.ProductQuote.Application.DTOs.Common;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Quotes
{
    public sealed class QuoteSearchRequest : PageRequest
    {
        public Guid? PropertyId { get; init; }


        public Guid? ProductId { get; init; }


        public QuoteStatus? Status { get; init; }


        public DateTime? GeneratedFromUtc { get; init; }


        public DateTime? GeneratedToUtc { get; init; }


        public bool? CustomerAccepted { get; init; }


        public AdminDecision? AdminDecision { get; init; }


        public bool? PaymentEligible { get; init; }
    }
}
