using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure.Persistence.Configurations
{
    public sealed class ClaimCoverageConfiguration
        : IEntityTypeConfiguration<ClaimCoverage>
    {
        public void Configure(
            EntityTypeBuilder<ClaimCoverage>
                builder)
        {
            builder.ToTable(
                "ClaimCoverages");


            builder.HasKey(
                coverage =>
                    coverage.Id);


            builder.Property(
                    coverage =>
                        coverage.CoverageName)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(
                    coverage =>
                        coverage.CoverageCharge)
                .HasPrecision(18, 2);


            builder.Property(
                    coverage =>
                        coverage
                            .CoverageLimitPercentage)
                .HasPrecision(9, 4);


            builder.Property(
                    coverage =>
                        coverage.CoverageLimitAmount)
                .HasPrecision(18, 2);


            builder.Property(
                    coverage =>
                        coverage.RequestedAmount)
                .HasPrecision(18, 2);


            builder.Property(
                    coverage =>
                        coverage.ApprovedAmount)
                .HasPrecision(18, 2);


            builder.HasIndex(
                    coverage =>
                        new
                        {
                            coverage.ClaimId,
                            coverage.CoverageId
                        })
                .IsUnique();


            builder.HasOne(
                    coverage =>
                        coverage.Claim)
                .WithMany(
                    claim =>
                        claim.Coverages)
                .HasForeignKey(
                    coverage =>
                        coverage.ClaimId)
                .OnDelete(
                    DeleteBehavior.Cascade);
        }
    }
}
