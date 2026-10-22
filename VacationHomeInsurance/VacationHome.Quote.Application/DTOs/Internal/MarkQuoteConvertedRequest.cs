namespace VacationHome.ProductQuote.Application.DTOs.Internal
{
    public sealed class MarkQuoteConvertedRequest
    {
        public Guid CustomerId { get; init; }


        public Guid PolicyId { get; init; }


        public Guid PaymentId { get; init; }


        public decimal PaidAmount { get; init; }


        public DateTime PaymentSucceededAtUtc { get; init; }


        public string CorrelationId { get; init; } =
            string.Empty;


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
