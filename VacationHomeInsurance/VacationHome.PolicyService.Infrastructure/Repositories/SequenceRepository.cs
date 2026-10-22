using Microsoft.EntityFrameworkCore;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Infrastructure.Persistence;


namespace VacationHome.PolicyService.Infrastructure.Repositories
{
    public sealed class SequenceRepository
        : ISequenceRepository
    {
        private const string PolicyNumber = "POLICY";
        private const string PaymentNumber = "PAYMENT";


        private readonly PolicyDbContext _context;


        public SequenceRepository(
            PolicyDbContext context)
        {
            _context = context;
        }


        public async Task<long> GetNextPolicyNumberAsync(
            CancellationToken cancellationToken)
        {
            return await GetNextAsync(
                PolicyNumber,
                cancellationToken);
        }


        public async Task<long> GetNextPaymentNumberAsync(
            CancellationToken cancellationToken)
        {
            return await GetNextAsync(
                PaymentNumber,
                cancellationToken);
        }


        private async Task<long> GetNextAsync(
            string name,
            CancellationToken cancellationToken)
        {
            var sequence =
                await _context.Set<Sequence>()
                    .FirstOrDefaultAsync(
                        x => x.Name == name,
                        cancellationToken);


            if (sequence is null)
            {
                sequence = new Sequence
                {
                    Name = name,
                    Value = 1
                };


                await _context.AddAsync(
                    sequence,
                    cancellationToken);
            }
            else
            {
                sequence.Value++;
            }


            await _context.SaveChangesAsync(
                cancellationToken);


            return sequence.Value;
        }
    }
}
