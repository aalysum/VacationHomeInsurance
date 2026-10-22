using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure.Persistence.Configurations
{
    public sealed class ClaimConfiguration
        : IEntityTypeConfiguration<Claim>
    {
        public void Configure(EntityTypeBuilder<Claim> builder)
        {
            builder.ToTable("Claims");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.ClaimNumber)
                .HasMaxLength(50)
                .IsRequired();


            builder.Property(x => x.PolicyNumber)
                .HasMaxLength(50)
                .IsRequired();


            builder.HasIndex(x => x.ClaimNumber)
                .IsUnique();
            builder.HasIndex(
    claim =>
        claim.AssignedClaimsManagerId);
            builder.HasIndex(
                claim =>
                    new
                    {
                        claim.AssignedClaimsManagerId,
                        claim.Status,
                        claim.SubmittedAtUtc
                    });
            builder.HasMany(
                    claim =>
                        claim.AssignmentHistory)
                .WithOne(
                    assignment =>
                        assignment.Claim)
                .HasForeignKey(
                    assignment =>
                        assignment.ClaimId)
                .OnDelete(
                    DeleteBehavior.Cascade);
            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(x => x.Description)
                .HasMaxLength(4000)
                .IsRequired();


            builder.Property(x => x.ClaimedAmount)
                .HasColumnType("decimal(18,2)");


            builder.Property(x => x.ApprovedAmount)
                .HasColumnType("decimal(18,2)");


            builder.Property(x => x.RejectionReason)
                .HasMaxLength(2000);


            builder.Property(x => x.InternalNotes)
                .HasMaxLength(4000);


            builder.Property(
                    claim =>
                        claim.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();


            builder.HasMany(x => x.Documents)
                .WithOne(x => x.Claim)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasMany(x => x.StatusHistory)
                .WithOne(x => x.Claim)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Property(
        claim =>
            claim.AssignedClaimsManagerId)
    .HasMaxLength(450);


            builder.Property(
                    claim =>
                        claim.AssignedClaimsManagerName)
                .HasMaxLength(200);


            builder.Property(
                    claim =>
                        claim.AssignedClaimsManagerEmail)
                .HasMaxLength(320);


            builder.Property(
                    claim =>
                        claim.AssignedBy)
                .HasMaxLength(450);
        }
    }
}
