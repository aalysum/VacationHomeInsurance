using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ProductQuote.Infrastructure.Persistence.Models;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Configurations
{
    public sealed class QuoteNumberSequenceConfiguration
        : IEntityTypeConfiguration<QuoteNumberSequence>
    {
        public void Configure(
            EntityTypeBuilder<QuoteNumberSequence> builder)
        {
            builder.ToTable(
                "QuoteNumberSequences",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_QuoteNumberSequences_Year",
                        "[Year] >= 2000 AND [Year] <= 9999");


                    tableBuilder.HasCheckConstraint(
                        "CK_QuoteNumberSequences_LastValue",
                        "[LastValue] >= 0");
                });


            builder.HasKey(x => x.Year);


            builder.Property(x => x.Year)
                .ValueGeneratedNever();


            builder.Property(x => x.LastValue)
                .IsRequired();


            builder.Property(x => x.UpdatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();
        }
    }
}
