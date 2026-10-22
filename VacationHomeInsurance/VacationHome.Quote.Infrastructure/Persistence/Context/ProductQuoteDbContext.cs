using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Infrastructure.Persistence.Models;


namespace VacationHome.ProductQuote.Infrastructure.Persistence
{
    public sealed class ProductQuoteDbContext : DbContext
    {
        public ProductQuoteDbContext(
            DbContextOptions<ProductQuoteDbContext> options)
            : base(options)
        {
        }


        public DbSet<AuditLog> AuditLogs =>
            Set<AuditLog>();


        public DbSet<Coverage> Coverages =>
            Set<Coverage>();


        public DbSet<PricingConfiguration> PricingConfigurations =>
            Set<PricingConfiguration>();


        public DbSet<Product> Products =>
            Set<Product>();


        public DbSet<ProductCoverage> ProductCoverages =>
            Set<ProductCoverage>();


        public DbSet<Quote> Quotes =>
            Set<Quote>();


        public DbSet<QuoteApprovalHistory> QuoteApprovalHistories =>
            Set<QuoteApprovalHistory>();


        public DbSet<QuoteCoverage> QuoteCoverages =>
            Set<QuoteCoverage>();


        public DbSet<RiskConfiguration> RiskConfigurations =>
            Set<RiskConfiguration>();


        public DbSet<QuoteNumberSequence> QuoteNumberSequences =>
            Set<QuoteNumberSequence>();


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ProductQuoteDbContext).Assembly);
        }
    }
}
