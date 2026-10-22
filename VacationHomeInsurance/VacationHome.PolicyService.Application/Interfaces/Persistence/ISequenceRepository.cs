namespace VacationHome.PolicyService.Application.Interfaces.Persistence
{
    public interface ISequenceRepository
    {
        Task<long> GetNextPolicyNumberAsync(
            CancellationToken cancellationToken);


        Task<long> GetNextPaymentNumberAsync(
            CancellationToken cancellationToken);
    }
}
