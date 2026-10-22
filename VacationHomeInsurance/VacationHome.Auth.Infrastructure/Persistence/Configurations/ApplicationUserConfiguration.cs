using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Infrastructure.Persistence.Configurations
{


    public class ApplicationUserConfiguration
        : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(
            EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.Status)
                .IsRequired();


            builder.Property(x => x.RefreshTokenVersion)
                .HasDefaultValue(0);


            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();
            //builder.Property(x => x.IsMfaEnabled)
            //    .HasDefaultValue(false);


            //builder.Property(x => x.MfaSecretKey)
            //    .HasMaxLength(500);
        }
    }
}
