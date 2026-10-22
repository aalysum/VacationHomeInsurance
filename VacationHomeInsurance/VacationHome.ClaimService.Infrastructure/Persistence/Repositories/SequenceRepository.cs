using Microsoft.EntityFrameworkCore;
using VacationHome.ClaimService.Application.Interfaces.Persistence;


namespace VacationHome.ClaimService.Infrastructure.Persistence.Repositories
{
    public sealed class SequenceRepository
        : ISequenceRepository
    {
        private readonly ClaimDbContext _context;


        public SequenceRepository(
            ClaimDbContext context)
        {
            _context = context;
        }


        public async Task<long> GetNextClaimNumberAsync(
            CancellationToken cancellationToken = default)
        {
            var sequence = await _context.ClaimSequences
                .SingleAsync(cancellationToken);


            sequence.NextValue++;


            return sequence.NextValue;
        }
    }
}
