using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Infrastructure.Persistence.Configurations
{
    public sealed class ClaimSequenceConfiguration
        : IEntityTypeConfiguration<ClaimSequence>
    {
        public void Configure(
            EntityTypeBuilder<ClaimSequence> builder)
        {
            builder.ToTable("ClaimSequences");


            builder.HasKey(x => x.Id);


            builder.HasData(
                new ClaimSequence
                {
                    Id = 1,
                    NextValue = 1000
                });
        }
    }
}
