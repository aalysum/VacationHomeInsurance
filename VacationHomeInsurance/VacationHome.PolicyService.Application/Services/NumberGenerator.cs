using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Application.Interfaces.Services;


namespace VacationHome.PolicyService.Application.Services
{
    public sealed class NumberGenerator
        : INumberGenerator
    {
        private readonly ISequenceRepository _sequenceRepository;


        public NumberGenerator(
            ISequenceRepository sequenceRepository)
        {
            _sequenceRepository = sequenceRepository;
        }


        public async Task<string> GeneratePolicyNumberAsync(
            CancellationToken cancellationToken)
        {
            var sequence =
                await _sequenceRepository
                    .GetNextPolicyNumberAsync(cancellationToken);


            return
                $"POL-{DateTime.UtcNow.Year}-{sequence:D5}";
        }


        public async Task<string> GeneratePaymentNumberAsync(
            CancellationToken cancellationToken)
        {
            var sequence =
                await _sequenceRepository
                    .GetNextPaymentNumberAsync(cancellationToken);


            return
                $"PAY-{DateTime.UtcNow.Year}-{sequence:D5}";
        }
    }
}
