using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Infrastructure.Persistence
{
    public sealed class PolicyDbContext : DbContext
    {
        public PolicyDbContext(
            DbContextOptions<PolicyDbContext> options)
            : base(options)
        {
        }


        public DbSet<Policy> Policies => Set<Policy>();


        public DbSet<PolicyDocument> PolicyDocuments =>
            Set<PolicyDocument>();


        public DbSet<PolicyCoverage> PolicyCoverages => Set<PolicyCoverage>();


        public DbSet<Payment> Payments => Set<Payment>();


        public DbSet<PolicyRenewal> PolicyRenewals => Set<PolicyRenewal>();


        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Sequence> Sequences => Set<Sequence>();


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(PolicyDbContext).Assembly);


            base.OnModelCreating(modelBuilder);
        }
    }
}
