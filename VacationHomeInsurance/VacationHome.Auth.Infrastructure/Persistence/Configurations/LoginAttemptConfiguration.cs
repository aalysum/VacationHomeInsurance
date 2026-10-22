using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Infrastructure.Persistence.Configurations
{


    public class LoginAttemptConfiguration
        : IEntityTypeConfiguration<LoginAttempt>
    {
        public void Configure(
            EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.HasKey(x => x.Id);


            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);
        }
    }
}
