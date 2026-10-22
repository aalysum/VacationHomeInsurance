using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Infrastructure.Persistence.Configurations
{
    public sealed class PolicyCoverageConfiguration
        : IEntityTypeConfiguration<PolicyCoverage>
    {
        public void Configure(
            EntityTypeBuilder<PolicyCoverage> builder)
        {
            builder.ToTable("PolicyCoverages");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.CoverageName)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.CoverageDescription)
                .HasMaxLength(2000)
                .IsRequired();


            builder.Property(x => x.CoverageCharge)
                .HasPrecision(18, 2);


            builder.Property(x => x.CoverageLimitPercentage)
                .HasPrecision(18, 2);


            builder.Property(x => x.CoverageLimitAmount)
                .HasPrecision(18, 2);
        }
    }
}
