using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.Auth.Domain.Entities;


public class PasswordResetTokenConfiguration
    : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(
        EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(x => x.Id);


        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);


        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(64);


        builder.HasIndex(x => x.TokenHash)
            .IsUnique();


        builder.HasIndex(x => new
        {
            x.UserId,
            x.UsedAtUtc,
            x.ExpiresAtUtc
        });
    }
}
