using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace VacationHome.ProductQuote.Infrastructure.Persistence
{
    public sealed class ProductQuoteDbContextFactory
        : IDesignTimeDbContextFactory<ProductQuoteDbContext>
    {
        public ProductQuoteDbContext CreateDbContext(
            string[] args)
        {
            string currentDirectory =
                Directory.GetCurrentDirectory();


            string apiProjectPath = Path.Combine(
                currentDirectory,
                "..",
                "VacationHome.ProductQuote.API");


            if (!Directory.Exists(apiProjectPath))
            {
                apiProjectPath = currentDirectory;
            }


            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(apiProjectPath)
                    .AddJsonFile(
                        "appsettings.json",
                        optional: false)
                    .AddJsonFile(
                        "appsettings.Development.json",
                        optional: true)
                    .AddEnvironmentVariables()
                    .Build();


            string connectionString =
                configuration.GetConnectionString(
                    "ProductQuoteDb")
                ?? throw new InvalidOperationException(
                    "Connection string 'ProductQuoteDb' was not configured.");


            var optionsBuilder =
                new DbContextOptionsBuilder<
                    ProductQuoteDbContext>();


            optionsBuilder.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(
                        typeof(ProductQuoteDbContext)
                            .Assembly
                            .FullName);
                });


            return new ProductQuoteDbContext(
                optionsBuilder.Options);
        }
    }
}
