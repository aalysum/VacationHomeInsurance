using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Infrastructure.Persistence.Configurations
{
    public sealed class PaymentConfiguration
        : IEntityTypeConfiguration<Payment>
    {
        public void Configure(
            EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.PaymentNumber)
                .HasMaxLength(30)
                .IsRequired();


            builder.HasIndex(x => x.PaymentNumber)
                .IsUnique();

            builder.HasIndex(x => x.QuoteId)
                .IsUnique()
                .HasDatabaseName("IX_Payments_QuoteId_Success")
                .HasFilter("[Status] = 2");


            builder.Property(x => x.CardholderName)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.Last4Digits)
                .HasMaxLength(4);


            builder.Property(x => x.FailureReason)
                .HasMaxLength(1000);


            builder.Property(x => x.Amount)
                .HasPrecision(18, 2);


            builder.Property(x => x.RowVersion)
                .IsConcurrencyToken();


            builder.HasOne(x => x.Policy)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
