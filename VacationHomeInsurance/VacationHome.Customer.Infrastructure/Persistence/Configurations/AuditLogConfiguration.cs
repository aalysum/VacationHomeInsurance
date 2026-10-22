using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Infrastructure
    .Persistence.Configurations
{
    public class AuditLogConfiguration
        : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(
            EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.EntityId)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.OldValues)
                .HasColumnType("nvarchar(max)");


            builder.Property(x => x.PerformedByRole)
                .HasMaxLength(100);


            builder.Property(x => x.CorrelationId)
                .HasMaxLength(100);


            builder.Property(x => x.PerformedAtUtc)
                .IsRequired();


            builder.HasIndex(x => x.EntityName);


            builder.HasIndex(x => x.EntityId);


            builder.HasIndex(x => x.PerformedAtUtc);
        }
    }
}
