using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Configurations
{
    public sealed class QuoteCoverageConfiguration
        : IEntityTypeConfiguration<QuoteCoverage>
    {
        public void Configure(
            EntityTypeBuilder<QuoteCoverage> builder)
        {
            builder.ToTable(
                "QuoteCoverages",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_QuoteCoverages_CoverageCharge",
                        "[CoverageCharge] >= 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_QuoteCoverages_CoverageLimitPercentage",
                        "[CoverageLimitPercentage] > 0 " +
                        "AND [CoverageLimitPercentage] <= 100");


                    tableBuilder.HasCheckConstraint(
                        "CK_QuoteCoverages_CoverageLimitAmount",
                        "[CoverageLimitAmount] >= 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_QuoteCoverages_DisplayOrder",
                        "[DisplayOrder] >= 0");
                });


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Id)
                .ValueGeneratedNever();


            builder.Property(x => x.QuoteId)
                .IsRequired();


            builder.Property(x => x.CoverageId)
                .IsRequired();


            builder.Property(x => x.ProductCoverageId)
                .IsRequired();


            builder.Property(x => x.CoverageName)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.CoverageDescription)
                .HasMaxLength(1000)
                .IsRequired();


            builder.Property(x => x.CoverageType)
                .HasConversion<int>()
                .IsRequired();


            builder.Property(x => x.CoverageCharge)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.CoverageLimitPercentage)
                .HasPrecision(9, 4)
                .IsRequired();


            builder.Property(x => x.CoverageLimitAmount)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.DisplayOrder)
                .IsRequired();


            builder.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();


            builder.HasOne(x => x.Quote)
                .WithMany(x => x.Coverages)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasIndex(x => new
            {
                x.QuoteId,
                x.CoverageId
            })
                .IsUnique();


            builder.HasIndex(x => new
            {
                x.QuoteId,
                x.DisplayOrder
            });
        }
    }
}
