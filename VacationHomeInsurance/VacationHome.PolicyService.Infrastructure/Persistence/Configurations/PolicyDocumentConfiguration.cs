using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Infrastructure.Persistence.Configurations
{
    public sealed class PolicyDocumentConfiguration
        : IEntityTypeConfiguration<PolicyDocument>
    {
        public void Configure(
            EntityTypeBuilder<PolicyDocument> builder)
        {
            builder.ToTable("PolicyDocuments");
            builder.HasKey(document => document.Id);

            builder.Property(document => document.StorageKey)
                .HasMaxLength(500);

            builder.Property(document => document.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(document => document.Sha256Checksum)
                .HasMaxLength(64);

            builder.Property(document => document.FailureCode)
                .HasMaxLength(100);

            builder.HasIndex(document => new
                {
                    document.PolicyId,
                    document.Version
                })
                .IsUnique();

            builder.HasIndex(document => new
                {
                    document.PolicyId,
                    document.Status
                });

            builder.HasOne(document => document.Policy)
                .WithMany(policy => policy.Documents)
                .HasForeignKey(document => document.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}