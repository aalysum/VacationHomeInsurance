using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace VacationHome.ProductQuote.Infrastructure.Persistence
{
    public static class ProductQuoteDatabaseInitializer
    {
        public static async Task InitializeProductQuoteDatabaseAsync(
            this IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            using IServiceScope scope =
                serviceProvider.CreateScope();


            ProductQuoteDbContext dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        ProductQuoteDbContext>();


            await dbContext.Database.EnsureCreatedAsync(
                cancellationToken);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID('dbo.Quotes', 'U') IS NOT NULL
                BEGIN
                    UPDATE dbo.Quotes
                    SET Status = 2
                    WHERE Status = 1;

                    IF EXISTS (
                        SELECT 1 FROM sys.indexes
                        WHERE name = 'IX_Quotes_QuoteGroupId_OptionNumber'
                          AND object_id = OBJECT_ID('dbo.Quotes'))
                        DROP INDEX IX_Quotes_QuoteGroupId_OptionNumber
                            ON dbo.Quotes;

                    IF EXISTS (
                        SELECT 1 FROM sys.indexes
                        WHERE name = 'IX_Quotes_QuoteGroupId_AlternativeRequestId'
                          AND object_id = OBJECT_ID('dbo.Quotes'))
                        DROP INDEX IX_Quotes_QuoteGroupId_AlternativeRequestId
                            ON dbo.Quotes;

                    IF EXISTS (
                        SELECT 1 FROM sys.check_constraints
                        WHERE name = 'CK_Quotes_OptionNumber'
                          AND parent_object_id = OBJECT_ID('dbo.Quotes'))
                        ALTER TABLE dbo.Quotes
                            DROP CONSTRAINT CK_Quotes_OptionNumber;

                    IF EXISTS (
                        SELECT 1 FROM sys.default_constraints
                        WHERE name = 'DF_Quotes_OptionNumber'
                          AND parent_object_id = OBJECT_ID('dbo.Quotes'))
                        ALTER TABLE dbo.Quotes
                            DROP CONSTRAINT DF_Quotes_OptionNumber;

                    IF COL_LENGTH('dbo.Quotes', 'QuoteGroupId') IS NOT NULL
                        ALTER TABLE dbo.Quotes DROP COLUMN QuoteGroupId;
                    IF COL_LENGTH('dbo.Quotes', 'OptionNumber') IS NOT NULL
                        ALTER TABLE dbo.Quotes DROP COLUMN OptionNumber;
                    IF COL_LENGTH('dbo.Quotes', 'AlternativeRequestId') IS NOT NULL
                        ALTER TABLE dbo.Quotes DROP COLUMN AlternativeRequestId;
                    IF COL_LENGTH('dbo.Quotes', 'SelectedQuoteId') IS NOT NULL
                        ALTER TABLE dbo.Quotes DROP COLUMN SelectedQuoteId;
                END;
                """,
                cancellationToken);


            ProductQuoteDatabaseSeeder seeder =
                scope.ServiceProvider
                    .GetRequiredService<
                        ProductQuoteDatabaseSeeder>();


            await seeder.SeedAsync(
                cancellationToken);
        }
    }
}
