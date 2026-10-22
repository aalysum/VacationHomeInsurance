using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Infrastructure.Persistence.Configurations
{
    public sealed class AuditLogConfiguration
        : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(
            EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Action)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.EntityType)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.EntityId)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.Details)
                .HasMaxLength(4000);
        }
    }
}
