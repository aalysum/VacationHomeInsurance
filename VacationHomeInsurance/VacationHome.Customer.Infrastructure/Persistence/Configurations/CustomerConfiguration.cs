using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Infrastructure
    .Persistence.Configurations
{
    public class CustomerConfiguration
        : IEntityTypeConfiguration<Customers>
    {
        public void Configure(
            EntityTypeBuilder<Customers> builder)
        {
            builder.ToTable("Customers");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.AuthUserId)
                .IsRequired()
                .HasMaxLength(450);


            builder.HasIndex(x => x.AuthUserId)
                .IsUnique();


            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);


            builder.Property(x => x.Status)
                .IsRequired();


            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();


            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired(false);


            builder.HasCheckConstraint(
                "CK_Customers_Status",
                "[Status] IN (1, 2)");
        }
    }
}
