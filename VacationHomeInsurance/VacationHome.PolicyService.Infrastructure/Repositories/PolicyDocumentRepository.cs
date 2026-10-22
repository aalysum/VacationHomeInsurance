using Microsoft.EntityFrameworkCore;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Infrastructure.Persistence;


namespace VacationHome.PolicyService.Infrastructure.Repositories
{
    public sealed class PolicyDocumentRepository
        : IPolicyDocumentRepository
    {
        private readonly PolicyDbContext _context;

        public PolicyDocumentRepository(PolicyDbContext context)
        {
            _context = context;
        }

        public Task<PolicyDocument?> GetByIdAsync(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            return _context.PolicyDocuments
                .SingleOrDefaultAsync(
                    document =>
                        document.PolicyId == policyId &&
                        document.Id == documentId,
                    cancellationToken);
        }

        public Task<PolicyDocument?> GetLatestAsync(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            return _context.PolicyDocuments
                .Where(document => document.PolicyId == policyId)
                .OrderByDescending(document => document.Version)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PolicyDocument>> GetAllAsync(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            return await _context.PolicyDocuments
                .AsNoTracking()
                .Where(document => document.PolicyId == policyId)
                .OrderByDescending(document => document.Version)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            PolicyDocument document,
            CancellationToken cancellationToken)
        {
            await _context.PolicyDocuments.AddAsync(
                document,
                cancellationToken);
        }


        public Task UpdateAsync(
            PolicyDocument document,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _context.PolicyDocuments.Update(document);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}