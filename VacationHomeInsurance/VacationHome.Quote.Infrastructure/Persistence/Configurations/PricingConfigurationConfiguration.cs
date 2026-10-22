using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Configurations
{
    public sealed class PricingConfigurationConfiguration
        : IEntityTypeConfiguration<PricingConfiguration>
    {
        public void Configure(
            EntityTypeBuilder<PricingConfiguration> builder)
        {
            builder.ToTable(
                "PricingConfigurations",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_PricingConfigurations_BaseRate",
                        "[BaseRate] >= 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_PricingConfigurations_LowRiskMultiplier",
                        "[LowRiskMultiplier] > 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_PricingConfigurations_MediumRiskMultiplier",
                        "[MediumRiskMultiplier] > 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_PricingConfigurations_HighRiskMultiplier",
                        "[HighRiskMultiplier] > 0");
                });


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Id)
                .ValueGeneratedNever();


            builder.Property(x => x.BaseRate)
                .HasPrecision(18, 6)
                .IsRequired();


            builder.Property(x => x.LowRiskMultiplier)
                .HasPrecision(18, 6)
                .IsRequired();


            builder.Property(x => x.MediumRiskMultiplier)
                .HasPrecision(18, 6)
                .IsRequired();


            builder.Property(x => x.HighRiskMultiplier)
                .HasPrecision(18, 6)
                .IsRequired();


            builder.Property(x => x.IsActive)
                .IsRequired();


            builder.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();


            builder.Property(x => x.UpdatedAtUtc)
                .HasColumnType("datetime2");


            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();


            builder.HasIndex(x => x.IsActive);
        }
    }
}
