namespace VacationHome.ClaimService.Application.Interfaces.Persistence
{
    public interface ISequenceRepository
    {
        Task<long> GetNextClaimNumberAsync(
            CancellationToken cancellationToken = default);
    }
}
