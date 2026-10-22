using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure
    .Persistence.Configurations
{
    public sealed class
        ClaimAssignmentHistoryConfiguration
        : IEntityTypeConfiguration<
            ClaimAssignmentHistory>
    {
        public void Configure(
            EntityTypeBuilder<
                ClaimAssignmentHistory> builder)
        {
            builder.ToTable(
                "ClaimAssignmentHistories");


            builder.HasKey(
                assignment =>
                    assignment.Id);


            builder.Property(
                    assignment =>
                        assignment
                            .PreviousClaimsManagerId)
                .HasMaxLength(450);


            builder.Property(
                    assignment =>
                        assignment
                            .PreviousClaimsManagerName)
                .HasMaxLength(200);


            builder.Property(
                    assignment =>
                        assignment
                            .PreviousClaimsManagerEmail)
                .HasMaxLength(320);


            builder.Property(
                    assignment =>
                        assignment.ClaimsManagerId)
                .HasMaxLength(450)
                .IsRequired();


            builder.Property(
                    assignment =>
                        assignment.ClaimsManagerName)
                .HasMaxLength(200)
                .IsRequired();


            builder.Property(
                    assignment =>
                        assignment.ClaimsManagerEmail)
                .HasMaxLength(320)
                .IsRequired();


            builder.Property(
                    assignment =>
                        assignment.Reason)
                .HasMaxLength(2000);


            builder.Property(
                    assignment =>
                        assignment.AssignedBy)
                .HasMaxLength(450)
                .IsRequired();


            builder.Property(
                    assignment =>
                        assignment.AssignedAtUtc)
                .IsRequired();


            builder.HasOne(
                    assignment =>
                        assignment.Claim)
                .WithMany(
                    claim =>
                        claim.AssignmentHistory)
                .HasForeignKey(
                    assignment =>
                        assignment.ClaimId)
                .OnDelete(
                    DeleteBehavior.Cascade);


            builder.HasIndex(
                assignment =>
                    assignment.ClaimId);


            builder.HasIndex(
                assignment =>
                    assignment.ClaimsManagerId);


            builder.HasIndex(
                assignment =>
                    assignment.AssignedAtUtc);
        }
    }
}
