using Microsoft.EntityFrameworkCore;
using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Infrastructure
    .Persistence.Context
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(
            DbContextOptions<CustomerDbContext> options)
            : base(options)
        {
        }


        public DbSet<Customers> Customers =>
            Set<Customers>();


        public DbSet<CustomerProfile> CustomerProfiles =>
            Set<CustomerProfile>();


        public DbSet<VacationProperty> VacationProperties =>
            Set<VacationProperty>();


        public DbSet<AuditLog> AuditLogs =>
            Set<AuditLog>();




        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(CustomerDbContext).Assembly);
        }
    }
}
