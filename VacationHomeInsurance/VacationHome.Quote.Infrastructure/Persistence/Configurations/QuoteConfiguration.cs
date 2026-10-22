using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Configurations
{
    public sealed class QuoteConfiguration
        : IEntityTypeConfiguration<Quote>
    {
        public void Configure(
            EntityTypeBuilder<Quote> builder)
        {
            builder.ToTable(
                "Quotes",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_Quotes_PropertyValue",
                        "[PropertyValue] > 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_Quotes_ConstructionYear",
                        "[ConstructionYear] > 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_Quotes_PropertyAge",
                        "[PropertyAge] >= 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_Quotes_PreviousClaimsCount",
                        "[PreviousClaimsCount] >= 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_Quotes_RiskScore",
                        "[RiskScore] >= 0 AND [RiskScore] <= 100");


                    tableBuilder.HasCheckConstraint(
                        "CK_Quotes_FinalAnnualPremium",
                        "[FinalAnnualPremium] > 0");


                    tableBuilder.HasCheckConstraint(
                        "CK_Quotes_Expiry",
                        "[ExpiresAtUtc] > [GeneratedAtUtc]");

                });


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Id)
                .ValueGeneratedNever();


            builder.Property(x => x.QuoteNumber)
                .HasMaxLength(50)
                .IsRequired();


            builder.Property(x => x.CustomerId)
                .IsRequired();


            builder.Property(x => x.PropertyId)
                .IsRequired();


            builder.Property(x => x.ProductId)
                .IsRequired();


            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.GeneratedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();


            builder.Property(x => x.ExpiresAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();


            builder.Property(x => x.ExpiredAtUtc)
                .HasColumnType("datetime2");


            builder.Property(x => x.PaymentDeadlineUtc)
                .HasColumnType("datetime2");


            builder.Property(x => x.ProductType)
                .HasConversion<int>()
                .IsRequired();


            builder.Property(x => x.ProductName)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.ProductDescription)
                .HasMaxLength(1000)
                .IsRequired();


            builder.Property(x => x.PropertyName)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.PropertyType)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.AddressLine1)
                .HasMaxLength(300)
                .IsRequired();


            builder.Property(x => x.AddressLine2)
                .HasMaxLength(300);


            builder.Property(x => x.City)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.State)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.PostalCode)
                .HasMaxLength(30)
                .IsRequired();


            builder.Property(x => x.Country)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.PropertyValue)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.ConstructionYear)
                .IsRequired();


            builder.Property(x => x.PropertyAge)
                .IsRequired();


            builder.Property(x => x.OccupancyFrequency)
                .HasMaxLength(50)
                .IsRequired();


            builder.Property(x => x.HasSecuritySystem)
                .IsRequired();


            builder.Property(x => x.PreviousClaimsCount)
                .IsRequired();


            builder.Property(x => x.HasDisasterExposure)
                .IsRequired();


            builder.Property(x => x.RiskConfigurationId)
                .IsRequired();


            builder.Property(x => x.MediumPropertyValueThreshold)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.HighPropertyValueThreshold)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.PropertyAgePoints)
                .IsRequired();


            builder.Property(x => x.PreviousClaimsPoints)
                .IsRequired();


            builder.Property(x => x.DisasterExposurePoints)
                .IsRequired();


            builder.Property(x => x.SecuritySystemPoints)
                .IsRequired();


            builder.Property(x => x.OccupancyPoints)
                .IsRequired();


            builder.Property(x => x.PropertyValuePoints)
                .IsRequired();


            builder.Property(x => x.RiskScore)
                .IsRequired();


            builder.Property(x => x.RiskLevel)
                .HasConversion<int>()
                .IsRequired();


            builder.Property(x => x.RiskExplanation)
                .HasMaxLength(2000)
                .IsRequired();


            builder.Property(x => x.PricingConfigurationId)
                .IsRequired();


            builder.Property(x => x.BaseRate)
                .HasPrecision(18, 6)
                .IsRequired();


            builder.Property(x => x.RiskMultiplier)
                .HasPrecision(18, 6)
                .IsRequired();


            builder.Property(x => x.BasePremium)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.RiskAdjustedPremium)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.RiskAdjustmentAmount)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.MandatoryCoverageCharges)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.OptionalCoverageCharges)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.FinalAnnualPremium)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.EstimatedMonthlyEquivalent)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.PaymentFrequency)
                .HasMaxLength(50)
                .IsRequired();


            builder.Property(x => x.AcceptanceCorrelationId)
                .HasMaxLength(100);


            builder.Property(x => x.AdminDecision)
                .HasConversion<int?>();


            builder.Property(x => x.AdminDecisionAtUtc)
                .HasColumnType("datetime2");


            builder.Property(x => x.ApprovalRemarks)
                .HasMaxLength(1000);


            builder.Property(x => x.RejectionReason)
                .HasMaxLength(1000);


            builder.Property(x => x.AdminDecisionCorrelationId)
                .HasMaxLength(100);


            builder.Property(x => x.WithdrawnAtUtc)
                .HasColumnType("datetime2");


            builder.Property(x => x.WithdrawalReason)
                .HasMaxLength(1000);


            builder.Property(x => x.WithdrawalCorrelationId)
                .HasMaxLength(100);


            builder.Property(x => x.InvalidatedAtUtc)
                .HasColumnType("datetime2");


            builder.Property(x => x.InvalidationReason)
                .HasMaxLength(1000);


            builder.Property(x => x.InvalidationCorrelationId)
                .HasMaxLength(100);


            builder.Property(x => x.ConvertedAtUtc)
                .HasColumnType("datetime2");


            builder.Property(x => x.ConversionCorrelationId)
                .HasMaxLength(100);


            builder.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();


            builder.Property(x => x.UpdatedAtUtc)
                .HasColumnType("datetime2");


            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();


            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasMany(x => x.Coverages)
                .WithOne(x => x.Quote)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasMany(x => x.ApprovalHistory)
                .WithOne(x => x.Quote)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasIndex(x => x.QuoteNumber)
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.CustomerId,
                x.GeneratedAtUtc
            });


            builder.HasIndex(x => new
            {
                x.CustomerId,
                x.Status
            });


            builder.HasIndex(x => new
            {
                x.PropertyId,
                x.Status
            });


            builder.HasIndex(x => new
            {
                x.ProductId,
                x.Status
            });


            builder.HasIndex(x => new
            {
                x.Status,
                x.ExpiresAtUtc
            });


            builder.HasIndex(x => new
            {
                x.Status,
                x.PaymentDeadlineUtc
            });


            builder.HasIndex(x => x.ConvertedPolicyId);


            builder.HasIndex(x => x.PricingConfigurationId);


            builder.HasIndex(x => x.RiskConfigurationId);
        }
    }
}
