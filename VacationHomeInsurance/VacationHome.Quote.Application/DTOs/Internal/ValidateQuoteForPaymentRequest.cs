namespace VacationHome.ProductQuote.Application.DTOs.Internal
{
    public sealed class ValidateQuoteForPaymentRequest
    {
        public Guid QuoteId { get; init; }


        public Guid CustomerId { get; init; }


        public decimal PaymentAmount { get; init; }


        public string CorrelationId { get; init; } =
            string.Empty;
    }
}
