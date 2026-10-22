using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Configurations
{
    public sealed class ProductConfiguration
        : IEntityTypeConfiguration<Product>
    {
        public void Configure(
            EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Id)
                .ValueGeneratedNever();


            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.Description)
                .HasMaxLength(1000)
                .IsRequired();


            builder.Property(x => x.ProductType)
                .HasConversion<int>()
                .IsRequired();


            builder.Property(x => x.IsActive)
                .IsRequired();


            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();


            builder.HasIndex(x => x.Name)
                .IsUnique();


            builder.HasIndex(x => x.ProductType)
                .IsUnique();


            builder.HasIndex(x => x.IsActive);
        }
    }
}
