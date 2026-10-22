namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Models
{
    public sealed class QuoteNumberSequence
    {
        public int Year { get; set; }


        public long LastValue { get; set; }


        public DateTime UpdatedAtUtc { get; set; }
    }
}
