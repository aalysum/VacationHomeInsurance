using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure.Persistence.Configurations
{
    public sealed class AuditLogConfiguration
        : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(
            EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.EntityType)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.EntityId)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.PerformedBy)
                .HasMaxLength(256);


            builder.Property(x => x.Details)
                .HasMaxLength(4000);


            builder.HasIndex(x =>
                new
                {
                    x.EntityType,
                    x.EntityId
                });
        }
    }
}
