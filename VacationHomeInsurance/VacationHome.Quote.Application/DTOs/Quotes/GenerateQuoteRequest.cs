namespace VacationHome.ProductQuote.Application.DTOs.Quotes
{
    public sealed class GenerateQuoteRequest
    {
        public Guid PropertyId { get; init; }


        public Guid ProductId { get; init; }


        public IReadOnlyCollection<Guid>
            SelectedOptionalCoverageIds
        {
            get;
            init;
        } = Array.Empty<Guid>();
    }
}
