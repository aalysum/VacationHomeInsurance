using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure.Persistence.Configurations
{
    public sealed class ClaimStatusHistoryConfiguration
        : IEntityTypeConfiguration<ClaimStatusHistory>
    {
        public void Configure(
            EntityTypeBuilder<ClaimStatusHistory> builder)
        {
            builder.ToTable("ClaimStatusHistories");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Comments)
                .HasMaxLength(4000);


            builder.Property(x => x.ChangedBy)
                .HasMaxLength(256)
                .IsRequired();
        }
    }
}
