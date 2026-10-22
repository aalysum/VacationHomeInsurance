namespace VacationHome.PolicyService.Application.Interfaces.Services
{
    public interface INumberGenerator
    {
        Task<string> GeneratePolicyNumberAsync(
            CancellationToken cancellationToken);


        Task<string> GeneratePaymentNumberAsync(
            CancellationToken cancellationToken);
    }
}
