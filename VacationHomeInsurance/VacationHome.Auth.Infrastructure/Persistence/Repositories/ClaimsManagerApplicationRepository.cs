using Microsoft.EntityFrameworkCore;
using VacationHome.Auth.Application.Interfaces.Repositories;
using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Domain.Enums;
using VacationHome.Auth.Infrastructure.Persistence.Context;


namespace VacationHome.Auth.Infrastructure.Persistence.Repositories
{


    public class ClaimsManagerApplicationRepository
        : IClaimsManagerApplicationRepository
    {
        private readonly AuthDbContext _dbContext;


        public ClaimsManagerApplicationRepository(
            AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task AddAsync(
            ClaimsManagerApplication entity,
            CancellationToken cancellationToken)
        {
            await _dbContext.ClaimsManagerApplications
                .AddAsync(entity, cancellationToken);
        }


        public async Task<ClaimsManagerApplication?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.ClaimsManagerApplications
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }


        public async Task<List<
            ClaimsManagerApplication>>
            GetPendingAsync(
                CancellationToken cancellationToken)
        {
            return await _dbContext
                .ClaimsManagerApplications
                .AsNoTracking()
                .Where(
                    x =>
                        x.Status ==
                        ClaimsManagerApprovalStatus
                            .PendingApproval)
                .OrderBy(
                    x => x.CreatedAtUtc)
                .ToListAsync(
                    cancellationToken);
        }
        public Task UpdateAsync(
            ClaimsManagerApplication entity,
            CancellationToken cancellationToken)
        {
            _dbContext.ClaimsManagerApplications.Update(entity);


            return Task.CompletedTask;
        }
        public async Task<ClaimsManagerApplication?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.ClaimsManagerApplications
                .FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);
        }


        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
