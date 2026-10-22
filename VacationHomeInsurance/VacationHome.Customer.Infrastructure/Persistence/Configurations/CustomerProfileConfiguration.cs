using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Infrastructure
    .Persistence.Configurations
{
    public class CustomerProfileConfiguration
        : IEntityTypeConfiguration<CustomerProfile>
    {
        public void Configure(
            EntityTypeBuilder<CustomerProfile> builder)
        {
            builder.ToTable("CustomerProfiles");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.CustomerId)
                .IsRequired();


            builder.HasIndex(x => x.CustomerId)
                .IsUnique();


            builder.Property(x => x.DateOfBirth)
                .HasColumnType("date")
                .IsRequired(false);


            builder.Property(x => x.AddressLine1)
                .HasMaxLength(200);


            builder.Property(x => x.AddressLine2)
                .HasMaxLength(200);


            builder.Property(x => x.City)
                .HasMaxLength(100);


            builder.Property(x => x.State)
                .HasMaxLength(100);


            builder.Property(x => x.PostalCode)
                .HasMaxLength(20);


            builder.Property(x => x.Country)
                .HasMaxLength(100);


            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();


            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired(false);


            builder.HasOne(x => x.Customer)
                .WithOne(x => x.Profile)
                .HasForeignKey<CustomerProfile>(
                    x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
