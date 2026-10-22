using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ProductQuote.Domain.Entities;


namespace VacationHome.ProductQuote.Infrastructure
    .Persistence.Configurations
{
    public sealed class AuditLogConfiguration
        : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(
            EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Id)
                .ValueGeneratedNever();


            builder.Property(x => x.EntityName)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.EntityId)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.Action)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.OldValues)
                .HasColumnType("nvarchar(max)");


            builder.Property(x => x.NewValues)
                .HasColumnType("nvarchar(max)");


            builder.Property(x => x.PerformedByRole)
                .HasMaxLength(100);


            builder.Property(x => x.CorrelationId)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.PerformedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();


            builder.HasIndex(x => x.EntityName);


            builder.HasIndex(x => x.EntityId);


            builder.HasIndex(x => x.CorrelationId);


            builder.HasIndex(x => x.PerformedAtUtc);
        }
    }
}
