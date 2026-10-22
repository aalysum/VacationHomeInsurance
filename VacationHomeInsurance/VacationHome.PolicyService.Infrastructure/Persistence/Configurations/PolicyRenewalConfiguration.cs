using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Infrastructure.Persistence.Configurations
{
    public sealed class PolicyRenewalConfiguration
        : IEntityTypeConfiguration<PolicyRenewal>
    {
        public void Configure(
            EntityTypeBuilder<PolicyRenewal> builder)
        {
            builder.ToTable("PolicyRenewals");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.RowVersion)
                .IsConcurrencyToken();
        }
    }
}
