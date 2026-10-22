namespace VacationHome.ProductQuote.Application
    .Interfaces.Persistence
{
    public interface ISequenceRepository
    {
        Task<long> GetNextQuoteSequenceAsync(
            int year,
            CancellationToken cancellationToken);
    }
}
