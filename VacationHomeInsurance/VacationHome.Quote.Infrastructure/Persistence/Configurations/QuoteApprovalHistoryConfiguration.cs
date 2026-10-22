using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Configurations
{
    public sealed class QuoteApprovalHistoryConfiguration
        : IEntityTypeConfiguration<QuoteApprovalHistory>
    {
        public void Configure(
            EntityTypeBuilder<QuoteApprovalHistory> builder)
        {
            builder.ToTable("QuoteApprovalHistories");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Id)
                .ValueGeneratedNever();


            builder.Property(x => x.QuoteId)
                .IsRequired();


            builder.Property(x => x.AdminId)
                .IsRequired();


            builder.Property(x => x.Decision)
                .HasConversion<int>()
                .IsRequired();


            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);


            builder.Property(x => x.RejectionReason)
                .HasMaxLength(1000);


            builder.Property(x => x.CorrelationId)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.DecisionAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();


            builder.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();


            builder.HasOne(x => x.Quote)
                .WithMany(x => x.ApprovalHistory)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasIndex(x => new
            {
                x.QuoteId,
                x.DecisionAtUtc
            });


            builder.HasIndex(x => x.AdminId);


            builder.HasIndex(x => x.CorrelationId);
        }
    }
}
