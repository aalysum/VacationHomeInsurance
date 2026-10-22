using Microsoft.EntityFrameworkCore;
using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure.Persistence.Repositories
{
    public sealed class ClaimDocumentRepository
        : IClaimDocumentRepository
    {
        private readonly ClaimDbContext _context;


        public ClaimDocumentRepository(
            ClaimDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            ClaimDocument document,
            CancellationToken cancellationToken = default)
        {
            await _context.ClaimDocuments.AddAsync(
                document,
                cancellationToken);
        }


        public async Task<ClaimDocument?> GetByIdAsync(
            Guid id,
            bool track = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<ClaimDocument> query =
                _context.ClaimDocuments;


            if (!track)
            {
                query = query.AsNoTracking();
            }


            return await query.FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
        }


        public async Task<IReadOnlyCollection<ClaimDocument>>
            GetByClaimIdAsync(
                Guid claimId,
                CancellationToken cancellationToken = default)
        {
            return await _context.ClaimDocuments
                .AsNoTracking()
                .Where(x => x.ClaimId == claimId)
                .OrderByDescending(x => x.UploadedAtUtc)
                .ToListAsync(cancellationToken);
        }




        public Task DeleteAsync(
            ClaimDocument document,
            CancellationToken cancellationToken = default)
        {
            _context.ClaimDocuments.Remove(document);


            return Task.CompletedTask;
        }
    }
}
