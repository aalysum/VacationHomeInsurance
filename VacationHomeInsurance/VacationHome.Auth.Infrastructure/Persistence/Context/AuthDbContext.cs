using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Infrastructure.Persistence.Context
{


    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }


        public DbSet<ClaimsManagerApplication> ClaimsManagerApplications =>
            Set<ClaimsManagerApplication>();


        public DbSet<RefreshToken> RefreshTokens =>
            Set<RefreshToken>();


        public DbSet<PasswordResetToken> PasswordResetTokens =>
            Set<PasswordResetToken>();


        public DbSet<LoginAttempt> LoginAttempts =>
            Set<LoginAttempt>();


        public DbSet<AuditLog> AuditLogs =>
            Set<AuditLog>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.ApplyConfigurationsFromAssembly(
                typeof(AuthDbContext).Assembly);
        }
    }
}
