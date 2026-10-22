using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.DTOs.Internal
{
    public sealed class QuoteConversionResultDto
    {
        public Guid QuoteId { get; init; }


        public string QuoteNumber { get; init; } =
            string.Empty;


        public Guid PolicyId { get; init; }


        public QuoteStatus Status { get; init; }


        public bool WasAlreadyConverted { get; init; }


        public DateTime ConvertedAtUtc { get; init; }


        public string CorrelationId { get; init; } =
            string.Empty;


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
