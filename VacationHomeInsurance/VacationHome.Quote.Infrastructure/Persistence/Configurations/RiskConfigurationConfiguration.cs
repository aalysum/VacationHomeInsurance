using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Configurations
{
    public sealed class RiskConfigurationConfiguration
        : IEntityTypeConfiguration<RiskConfiguration>
    {
        public void Configure(
            EntityTypeBuilder<RiskConfiguration> builder)
        {
            builder.ToTable(
                "RiskConfigurations",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_RiskConfigurations_MediumThreshold",
                        "[MediumPropertyValueThreshold] > 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_RiskConfigurations_HighThreshold",
                        "[HighPropertyValueThreshold] > " +
                        "[MediumPropertyValueThreshold]");
                });


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Id)
                .ValueGeneratedNever();


            builder.Property(x => x.MediumPropertyValueThreshold)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.HighPropertyValueThreshold)
                .HasPrecision(18, 2)
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
