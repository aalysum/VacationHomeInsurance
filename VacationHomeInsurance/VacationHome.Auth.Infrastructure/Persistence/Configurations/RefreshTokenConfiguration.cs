using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Infrastructure.Persistence.Configurations
{
    public sealed class RefreshTokenConfiguration
        : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(
            EntityTypeBuilder<RefreshToken> builder)
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
                x.RevokedAtUtc,
                x.ExpiresAtUtc
            });


            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();


            builder.Property(x => x.ExpiresAtUtc)
                .IsRequired();


            builder.Property(x => x.RowVersion)
                .IsRowVersion();


            builder.Ignore(x => x.IsActive);
        }
    }
}
