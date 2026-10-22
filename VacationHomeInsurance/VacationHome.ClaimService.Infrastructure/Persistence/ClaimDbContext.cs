using Microsoft.EntityFrameworkCore;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure.Persistence
{
    public sealed class ClaimDbContext
        : DbContext
    {
        public ClaimDbContext(
            DbContextOptions<ClaimDbContext> options)
            : base(options)
        {
        }


        public DbSet<Claim> Claims =>
            Set<Claim>();


        public DbSet<ClaimCoverage> ClaimCoverages =>
            Set<ClaimCoverage>();


        public DbSet<ClaimDocument> ClaimDocuments =>
            Set<ClaimDocument>();


        public DbSet<ClaimStatusHistory>
            ClaimStatusHistories =>
                Set<ClaimStatusHistory>();


        public DbSet<ClaimAssignmentHistory>
            ClaimAssignmentHistories =>
                Set<ClaimAssignmentHistory>();


        public DbSet<AuditLog> AuditLogs =>
            Set<AuditLog>();


        public DbSet<ClaimSequence> ClaimSequences =>
            Set<ClaimSequence>();


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder
                .ApplyConfigurationsFromAssembly(
                    typeof(ClaimDbContext)
                        .Assembly);


            base.OnModelCreating(
                modelBuilder);
        }
    }
}
