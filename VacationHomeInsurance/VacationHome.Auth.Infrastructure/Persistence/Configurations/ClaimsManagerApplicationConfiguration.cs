using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Infrastructure.Persistence.Configurations
{


    public class ClaimsManagerApplicationConfiguration
        : IEntityTypeConfiguration<ClaimsManagerApplication>
    {
        public void Configure(
            EntityTypeBuilder<ClaimsManagerApplication> builder)
        {
            builder.ToTable("ClaimsManagerApplications");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.UserId)
                .IsRequired();


            builder.HasIndex(x => x.UserId)
                .IsUnique();


            builder.Property(x => x.YearsOfExperience)
                .IsRequired();


            builder.Property(x => x.ProfessionalSummary)
                .IsRequired()
                .HasMaxLength(2000);


            builder.Property(x => x.VerificationDocumentPath)
                .IsRequired()
                .HasMaxLength(500);


            builder.Property(x => x.Status)
                .IsRequired();


            builder.Property(x => x.ApprovedByUserId)
                .HasMaxLength(450);


            builder.Property(x => x.RejectionReason)
                .HasMaxLength(1000);


            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();
            builder.Property(x => x.RowVersion)
                .IsRowVersion();
        }
    }
}
