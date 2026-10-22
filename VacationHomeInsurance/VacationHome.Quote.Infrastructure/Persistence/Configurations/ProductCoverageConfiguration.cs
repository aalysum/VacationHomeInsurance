using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Configurations
{
    public sealed class ProductCoverageConfiguration
        : IEntityTypeConfiguration<ProductCoverage>
    {
        public void Configure(
            EntityTypeBuilder<ProductCoverage> builder)
        {
            builder.ToTable(
                "ProductCoverages",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_ProductCoverages_CoverageCharge",
                        "[CoverageCharge] >= 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_ProductCoverages_CoverageLimitPercentage",
                        "[CoverageLimitPercentage] > 0 " +
                        "AND [CoverageLimitPercentage] <= 100");


                    tableBuilder.HasCheckConstraint(
                        "CK_ProductCoverages_DisplayOrder",
                        "[DisplayOrder] >= 0");
                });


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Id)
                .ValueGeneratedNever();


            builder.Property(x => x.ProductId)
                .IsRequired();


            builder.Property(x => x.CoverageId)
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


            builder.Property(x => x.DisplayOrder)
                .IsRequired();


            builder.Property(x => x.IsActive)
                .IsRequired();


            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();


            builder.HasOne(x => x.Product)
                .WithMany(x => x.Coverages)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(x => x.Coverage)
                .WithMany(x => x.ProductCoverages)
                .HasForeignKey(x => x.CoverageId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasIndex(x => new
            {
                x.ProductId,
                x.CoverageId
            })
                .IsUnique();


            builder.HasIndex(x => new
            {
                x.ProductId,
                x.IsActive,
                x.DisplayOrder
            });
        }
    }
}
