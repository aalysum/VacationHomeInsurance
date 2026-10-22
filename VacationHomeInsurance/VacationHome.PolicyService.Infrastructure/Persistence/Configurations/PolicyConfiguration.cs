using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Infrastructure.Persistence.Configurations
{
    public sealed class PolicyConfiguration
        : IEntityTypeConfiguration<Policy>
    {
        public void Configure(
            EntityTypeBuilder<Policy> builder)
        {
            builder.ToTable("Policies");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.PolicyNumber)
                .HasMaxLength(30)
                .IsRequired();


            builder.HasIndex(x => x.PolicyNumber)
                .IsUnique();

            builder.HasIndex(x => x.QuoteId)
                .IsUnique();


            builder.Property(x => x.ProductName)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.CustomerFirstName)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.CustomerLastName)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.CustomerPhoneNumber)
                .HasMaxLength(30)
                .IsRequired();


            builder.Property(x => x.PropertyName)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.PropertyAddressLine1)
                .HasMaxLength(250)
                .IsRequired();


            builder.Property(x => x.PropertyAddressLine2)
                .HasMaxLength(250);


            builder.Property(x => x.PropertyCity)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.PropertyState)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.PropertyPostalCode)
                .HasMaxLength(20)
                .IsRequired();


            builder.Property(x => x.PropertyCountry)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.PropertyType)
                .HasMaxLength(50)
                .IsRequired();


            builder.Property(x => x.PropertyValue)
                .HasPrecision(18, 2);


            builder.Property(x => x.BaseRate)
                .HasPrecision(18, 6);


            builder.Property(x => x.RiskMultiplier)
                .HasPrecision(18, 6);


            builder.Property(x => x.BasePremium)
                .HasPrecision(18, 2);


            builder.Property(x => x.RiskAdjustedPremium)
                .HasPrecision(18, 2);


            builder.Property(x => x.RiskAdjustmentAmount)
                .HasPrecision(18, 2);


            builder.Property(x => x.MandatoryCoverageCharges)
                .HasPrecision(18, 2);


            builder.Property(x => x.OptionalCoverageCharges)
                .HasPrecision(18, 2);


            builder.Property(x => x.PaymentFrequency)
                .HasMaxLength(30)
                .IsRequired();


            builder.Property(x => x.OccupancyFrequency)
                .HasMaxLength(30)
                .IsRequired();


            builder.Property(x => x.RiskLevel)
                .HasMaxLength(30)
                .IsRequired();


            builder.Property(x => x.RiskExplanation)
                .HasMaxLength(1000)
                .IsRequired();


            builder.Property(x => x.AnnualPremium)
                .HasPrecision(18, 2);


            builder.Property(x => x.CancellationReason)
                .HasMaxLength(1000);


            builder.Property(x => x.RowVersion)
                .IsConcurrencyToken();


            builder.HasMany(x => x.Coverages)
                .WithOne(x => x.Policy)
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasMany(x => x.Documents)
                .WithOne(x => x.Policy)
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Property(x => x.ProductType)
                .IsRequired();
        }
    }
}
