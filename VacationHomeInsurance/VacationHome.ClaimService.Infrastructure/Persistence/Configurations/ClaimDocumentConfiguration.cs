using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure.Persistence.Configurations
{
    public sealed class ClaimDocumentConfiguration
        : IEntityTypeConfiguration<ClaimDocument>
    {
        public void Configure(
            EntityTypeBuilder<ClaimDocument> builder)
        {
            builder.ToTable("ClaimDocuments");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.FileName)
                .HasMaxLength(255)
                .IsRequired();


            builder.Property(x => x.ContentType)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.StoragePath)
                .HasMaxLength(500)
                .IsRequired();


            builder.Property(x => x.UploadedBy)
                .HasMaxLength(256)
                .IsRequired();
        }
    }
}
