using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Application.Interfaces.Persistence
{
    public interface ICoverageRepository
    {
        Task<Coverage?> GetByIdAsync(
            Guid coverageId,
            bool tracking,
            CancellationToken cancellationToken);


        Task<IReadOnlyCollection<Coverage>> GetAllAsync(
            CancellationToken cancellationToken);


        void Update(Coverage coverage);
    }
}
