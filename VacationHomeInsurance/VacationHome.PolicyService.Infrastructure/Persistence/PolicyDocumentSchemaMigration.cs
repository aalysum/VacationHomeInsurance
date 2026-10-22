using Microsoft.EntityFrameworkCore;


namespace VacationHome.PolicyService.Infrastructure.Persistence
{
    public static class PolicyDocumentSchemaMigration
    {
        public static Task ApplyAsync(
            PolicyDbContext context,
            CancellationToken cancellationToken)
        {
            const string sql = """
                IF COL_LENGTH(N'dbo.Policies', N'CustomerFirstName') IS NULL
                    ALTER TABLE dbo.Policies ADD CustomerFirstName nvarchar(100) NOT NULL CONSTRAINT DF_Policies_CustomerFirstName DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'CustomerLastName') IS NULL
                    ALTER TABLE dbo.Policies ADD CustomerLastName nvarchar(100) NOT NULL CONSTRAINT DF_Policies_CustomerLastName DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'CustomerPhoneNumber') IS NULL
                    ALTER TABLE dbo.Policies ADD CustomerPhoneNumber nvarchar(30) NOT NULL CONSTRAINT DF_Policies_CustomerPhoneNumber DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PropertyAddressLine1') IS NULL
                    ALTER TABLE dbo.Policies ADD PropertyAddressLine1 nvarchar(250) NOT NULL CONSTRAINT DF_Policies_PropertyAddressLine1 DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PropertyAddressLine2') IS NULL
                    ALTER TABLE dbo.Policies ADD PropertyAddressLine2 nvarchar(250) NULL;
                IF COL_LENGTH(N'dbo.Policies', N'PropertyCity') IS NULL
                    ALTER TABLE dbo.Policies ADD PropertyCity nvarchar(100) NOT NULL CONSTRAINT DF_Policies_PropertyCity DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PropertyState') IS NULL
                    ALTER TABLE dbo.Policies ADD PropertyState nvarchar(100) NOT NULL CONSTRAINT DF_Policies_PropertyState DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PropertyPostalCode') IS NULL
                    ALTER TABLE dbo.Policies ADD PropertyPostalCode nvarchar(20) NOT NULL CONSTRAINT DF_Policies_PropertyPostalCode DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PropertyCountry') IS NULL
                    ALTER TABLE dbo.Policies ADD PropertyCountry nvarchar(100) NOT NULL CONSTRAINT DF_Policies_PropertyCountry DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PropertyType') IS NULL
                    ALTER TABLE dbo.Policies ADD PropertyType nvarchar(50) NOT NULL CONSTRAINT DF_Policies_PropertyType DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PropertyValue') IS NULL
                    ALTER TABLE dbo.Policies ADD PropertyValue decimal(18,2) NOT NULL CONSTRAINT DF_Policies_PropertyValue DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'BaseRate') IS NULL
                    ALTER TABLE dbo.Policies ADD BaseRate decimal(18,6) NOT NULL CONSTRAINT DF_Policies_BaseRate DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'RiskMultiplier') IS NULL
                    ALTER TABLE dbo.Policies ADD RiskMultiplier decimal(18,6) NOT NULL CONSTRAINT DF_Policies_RiskMultiplier DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'BasePremium') IS NULL
                    ALTER TABLE dbo.Policies ADD BasePremium decimal(18,2) NOT NULL CONSTRAINT DF_Policies_BasePremium DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'RiskAdjustedPremium') IS NULL
                    ALTER TABLE dbo.Policies ADD RiskAdjustedPremium decimal(18,2) NOT NULL CONSTRAINT DF_Policies_RiskAdjustedPremium DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'RiskAdjustmentAmount') IS NULL
                    ALTER TABLE dbo.Policies ADD RiskAdjustmentAmount decimal(18,2) NOT NULL CONSTRAINT DF_Policies_RiskAdjustmentAmount DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'MandatoryCoverageCharges') IS NULL
                    ALTER TABLE dbo.Policies ADD MandatoryCoverageCharges decimal(18,2) NOT NULL CONSTRAINT DF_Policies_MandatoryCoverageCharges DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'OptionalCoverageCharges') IS NULL
                    ALTER TABLE dbo.Policies ADD OptionalCoverageCharges decimal(18,2) NOT NULL CONSTRAINT DF_Policies_OptionalCoverageCharges DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PaymentFrequency') IS NULL
                    ALTER TABLE dbo.Policies ADD PaymentFrequency nvarchar(30) NOT NULL CONSTRAINT DF_Policies_PaymentFrequency DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'ConstructionYear') IS NULL
                    ALTER TABLE dbo.Policies ADD ConstructionYear int NOT NULL CONSTRAINT DF_Policies_ConstructionYear DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PropertyAge') IS NULL
                    ALTER TABLE dbo.Policies ADD PropertyAge int NOT NULL CONSTRAINT DF_Policies_PropertyAge DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'OccupancyFrequency') IS NULL
                    ALTER TABLE dbo.Policies ADD OccupancyFrequency nvarchar(30) NOT NULL CONSTRAINT DF_Policies_OccupancyFrequency DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'HasSecuritySystem') IS NULL
                    ALTER TABLE dbo.Policies ADD HasSecuritySystem bit NOT NULL CONSTRAINT DF_Policies_HasSecuritySystem DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PreviousClaimsCount') IS NULL
                    ALTER TABLE dbo.Policies ADD PreviousClaimsCount int NOT NULL CONSTRAINT DF_Policies_PreviousClaimsCount DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'DisasterExposure') IS NULL
                    ALTER TABLE dbo.Policies ADD DisasterExposure bit NOT NULL CONSTRAINT DF_Policies_DisasterExposure DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'RiskScore') IS NULL
                    ALTER TABLE dbo.Policies ADD RiskScore int NOT NULL CONSTRAINT DF_Policies_RiskScore DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'RiskLevel') IS NULL
                    ALTER TABLE dbo.Policies ADD RiskLevel nvarchar(30) NOT NULL CONSTRAINT DF_Policies_RiskLevel DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'RiskExplanation') IS NULL
                    ALTER TABLE dbo.Policies ADD RiskExplanation nvarchar(1000) NOT NULL CONSTRAINT DF_Policies_RiskExplanation DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'BaseRate') IS NULL
                    ALTER TABLE dbo.Policies ADD BaseRate decimal(18,6) NOT NULL CONSTRAINT DF_Policies_BaseRate DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'RiskMultiplier') IS NULL
                    ALTER TABLE dbo.Policies ADD RiskMultiplier decimal(18,6) NOT NULL CONSTRAINT DF_Policies_RiskMultiplier DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'BasePremium') IS NULL
                    ALTER TABLE dbo.Policies ADD BasePremium decimal(18,2) NOT NULL CONSTRAINT DF_Policies_BasePremium DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'RiskAdjustedPremium') IS NULL
                    ALTER TABLE dbo.Policies ADD RiskAdjustedPremium decimal(18,2) NOT NULL CONSTRAINT DF_Policies_RiskAdjustedPremium DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'RiskAdjustmentAmount') IS NULL
                    ALTER TABLE dbo.Policies ADD RiskAdjustmentAmount decimal(18,2) NOT NULL CONSTRAINT DF_Policies_RiskAdjustmentAmount DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'MandatoryCoverageCharges') IS NULL
                    ALTER TABLE dbo.Policies ADD MandatoryCoverageCharges decimal(18,2) NOT NULL CONSTRAINT DF_Policies_MandatoryCoverageCharges DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'OptionalCoverageCharges') IS NULL
                    ALTER TABLE dbo.Policies ADD OptionalCoverageCharges decimal(18,2) NOT NULL CONSTRAINT DF_Policies_OptionalCoverageCharges DEFAULT 0 WITH VALUES;
                IF COL_LENGTH(N'dbo.Policies', N'PaymentFrequency') IS NULL
                    ALTER TABLE dbo.Policies ADD PaymentFrequency nvarchar(30) NOT NULL CONSTRAINT DF_Policies_PaymentFrequency DEFAULT N'' WITH VALUES;
                IF COL_LENGTH(N'dbo.PolicyCoverages', N'CoverageDescription') IS NULL
                    ALTER TABLE dbo.PolicyCoverages ADD CoverageDescription nvarchar(2000) NOT NULL CONSTRAINT DF_PolicyCoverages_CoverageDescription DEFAULT N'' WITH VALUES;

                IF OBJECT_ID(N'dbo.PolicyDocuments', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.PolicyDocuments
                    (
                        Id uniqueidentifier NOT NULL CONSTRAINT PK_PolicyDocuments PRIMARY KEY,
                        PolicyId uniqueidentifier NOT NULL,
                        Version int NOT NULL,
                        StorageKey nvarchar(500) NULL,
                        ContentType nvarchar(100) NOT NULL,
                        FileSizeBytes bigint NOT NULL,
                        Sha256Checksum nvarchar(64) NULL,
                        Status int NOT NULL,
                        FailureCode nvarchar(100) NULL,
                        CreatedAtUtc datetime2 NOT NULL,
                        GeneratedAtUtc datetime2 NULL,
                        CONSTRAINT FK_PolicyDocuments_Policies_PolicyId
                            FOREIGN KEY (PolicyId) REFERENCES dbo.Policies(Id)
                            ON DELETE NO ACTION
                    );
                END;

                IF NOT EXISTS
                (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Policies_QuoteId'
                      AND object_id = OBJECT_ID(N'dbo.Policies')
                )
                    CREATE UNIQUE INDEX IX_Policies_QuoteId
                    ON dbo.Policies(QuoteId);

                IF NOT EXISTS
                (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Payments_QuoteId_Success'
                      AND object_id = OBJECT_ID(N'dbo.Payments')
                )
                    CREATE UNIQUE INDEX IX_Payments_QuoteId_Success
                    ON dbo.Payments(QuoteId)
                    WHERE Status = 2;

                IF NOT EXISTS
                (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'UX_PolicyDocuments_PolicyId_Version'
                      AND object_id = OBJECT_ID(N'dbo.PolicyDocuments')
                )
                    CREATE UNIQUE INDEX UX_PolicyDocuments_PolicyId_Version
                    ON dbo.PolicyDocuments(PolicyId, Version);

                IF NOT EXISTS
                (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_PolicyDocuments_PolicyId_Status'
                      AND object_id = OBJECT_ID(N'dbo.PolicyDocuments')
                )
                    CREATE INDEX IX_PolicyDocuments_PolicyId_Status
                    ON dbo.PolicyDocuments(PolicyId, Status);
                """;

            return context.Database.ExecuteSqlRawAsync(
                sql,
                cancellationToken);
        }
    }
}