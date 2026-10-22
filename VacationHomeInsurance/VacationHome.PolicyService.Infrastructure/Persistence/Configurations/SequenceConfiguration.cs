using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Infrastructure.Persistence.Configurations
{
    public sealed class SequenceConfiguration
        : IEntityTypeConfiguration<Sequence>
    {
        public void Configure(
            EntityTypeBuilder<Sequence> builder)
        {
            builder.ToTable("Sequences");


            builder.HasKey(x => x.Name);


            builder.Property(x => x.Name)
                .HasMaxLength(100);


            builder.Property(x => x.Value)
                .IsRequired();
        }
    }
}
