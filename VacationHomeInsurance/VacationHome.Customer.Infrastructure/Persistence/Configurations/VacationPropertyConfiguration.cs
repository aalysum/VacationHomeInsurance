using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Globalization;
using VacationHome.Customer.Domain.Constants;
using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Infrastructure
    .Persistence.Configurations
{
    public class VacationPropertyConfiguration
        : IEntityTypeConfiguration<VacationProperty>
    {
        public void Configure(
            EntityTypeBuilder<VacationProperty> builder)
        {
            builder.ToTable("VacationProperties");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.CustomerId)
                .IsRequired();


            builder.Property(x => x.PropertyName)
                .IsRequired()
                .HasMaxLength(200);


            builder.Property(x => x.PropertyType)
                .IsRequired();


            builder.Property(x => x.AddressLine1)
                .IsRequired()
                .HasMaxLength(200);


            builder.Property(x => x.AddressLine2)
                .HasMaxLength(200);


            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.State)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.PostalCode)
                .IsRequired()
                .HasMaxLength(20);


            builder.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.PropertyValue)
                .IsRequired()
                .HasColumnType("decimal(18,2)");


            builder.Property(x => x.ConstructionYear)
                .IsRequired();


            builder.Property(x => x.OccupancyFrequency)
                .IsRequired();


            builder.Property(x => x.HasSecuritySystem)
                .IsRequired();


            builder.Property(x => x.PreviousClaimsCount)
                .IsRequired();


            builder.Property(x => x.DisasterExposure)
                .IsRequired();


            builder.Property(x => x.IsArchived)
                .IsRequired()
                .HasDefaultValue(false);


            builder.Property(x => x.ArchivedAtUtc)
                .IsRequired(false);


            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();


            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired(false);


            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();


            builder.HasOne(x => x.Customer)
                .WithMany(x => x.VacationProperties)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasIndex(x => x.CustomerId);


            builder.HasIndex(
                x => new
                {
                    x.CustomerId,
                    x.IsArchived,
                    x.CreatedAtUtc
                })
                .HasDatabaseName(
                    "IX_VacationProperties_Customer_Archive_Created");


            builder.HasIndex(x => x.CreatedAtUtc);


            var minimumPropertyValue =
                CustomerDomainConstants.MinimumPropertyValue
                    .ToString(
                        CultureInfo.InvariantCulture);


            var maximumPropertyValue =
                CustomerDomainConstants.MaximumPropertyValue
                    .ToString(
                        CultureInfo.InvariantCulture);


            builder.HasCheckConstraint(
                "CK_VacationProperties_PropertyValue",
                $"[PropertyValue] >= {minimumPropertyValue} " +
                $"AND [PropertyValue] <= {maximumPropertyValue}");


            builder.HasCheckConstraint(
                "CK_VacationProperties_ConstructionYear",
                $"[ConstructionYear] >= " +
                $"{CustomerDomainConstants.MinimumConstructionYear}");


            builder.HasCheckConstraint(
                "CK_VacationProperties_PreviousClaimsCount",
                "[PreviousClaimsCount] >= 0");


            builder.HasCheckConstraint(
                "CK_VacationProperties_PropertyType",
                "[PropertyType] IN (1, 2, 3, 4)");


            builder.HasCheckConstraint(
                "CK_VacationProperties_OccupancyFrequency",
                "[OccupancyFrequency] IN (1, 2, 3)");


            builder.HasCheckConstraint(
                "CK_VacationProperties_ArchiveState",
                "([IsArchived] = 0 AND " +
                "[ArchivedAtUtc] IS NULL) OR " +
                "([IsArchived] = 1 AND " +
                "[ArchivedAtUtc] IS NOT NULL)");
        }
    }
}
