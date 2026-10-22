using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace VacationHome.PolicyService.Infrastructure.Persistence
{
    public sealed class PolicyDbContextFactory
        : IDesignTimeDbContextFactory<PolicyDbContext>
    {
        public PolicyDbContext CreateDbContext(
            string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<PolicyDbContext>();


            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=VacationHomePolicyDb;Trusted_Connection=True;TrustServerCertificate=True");


            return new PolicyDbContext(
                optionsBuilder.Options);
        }
    }
}
