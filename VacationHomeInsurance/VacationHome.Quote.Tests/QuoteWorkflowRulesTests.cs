using System.Globalization;
using VacationHome.ProductQuote.Application.DTOs.Admin;
using VacationHome.ProductQuote.Application.DTOs.Pricing;
using VacationHome.ProductQuote.Application.Exceptions;
using VacationHome.ProductQuote.Application.Mappers;
using VacationHome.ProductQuote.Application.Services;
using VacationHome.ProductQuote.Application.Validation;
using VacationHome.ProductQuote.Application.Validators;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;
using VacationHome.ProductQuote.Domain.ValueObjects;
using Xunit;

namespace VacationHome.ProductQuote.Tests;

public sealed class QuoteWorkflowRulesTests
{
    [Fact]
    public void PremiumCalculator_UsesIllustrativeConfiguredRatesAndCoverageCharges()
    {
        PremiumSnapshot result = new PremiumCalculator().Calculate(
            propertyValue: 900000m,
            riskLevel: RiskLevel.Low,
            baseRate: 0.005m,
            lowRiskMultiplier: 1m,
            mediumRiskMultiplier: 1.25m,
            highRiskMultiplier: 1.5m,
            mandatoryCoverageCharges: 425m,
            optionalCoverageCharges: 0m);

        Assert.Equal(4500m, result.BasePremium);
        Assert.Equal(0m, result.RiskAdjustmentAmount);
        Assert.Equal(425m, result.MandatoryCoverageCharges);
        Assert.Equal(4925m, result.FinalAnnualPremium);
        Assert.Equal(410.42m, result.EstimatedMonthlyEquivalent);
    }

    [Fact]
    public void PremiumCalculator_AddsMandatoryAndSelectedOptionalChargesOnce()
    {
        PremiumSnapshot result = new PremiumCalculator().Calculate(
            propertyValue: 900000m,
            riskLevel: RiskLevel.Low,
            baseRate: 0.005m,
            lowRiskMultiplier: 1m,
            mediumRiskMultiplier: 1.25m,
            highRiskMultiplier: 1.5m,
            mandatoryCoverageCharges: 325m,
            optionalCoverageCharges: 100m);

        Assert.Equal(325m, result.MandatoryCoverageCharges);
        Assert.Equal(100m, result.OptionalCoverageCharges);
        Assert.Equal(4925m, result.FinalAnnualPremium);
    }

    [Theory]
    [InlineData("4925", "410.42")]
    [InlineData("1000.05", "83.34")]
    [InlineData("0.06", "0.01")]
    public void MonthlyEquivalent_UsesAnnualPremiumAndInrRounding(
        string annualPremium,
        string expectedMonthlyEquivalent)
    {
        Assert.Equal(
            decimal.Parse(
                expectedMonthlyEquivalent,
                CultureInfo.InvariantCulture),
            PremiumCalculator.CalculateMonthlyEquivalent(
                decimal.Parse(
                    annualPremium,
                    CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void MonthlyEquivalent_RejectsInvalidAnnualPremium()
    {
        Assert.Throws<BadRequestException>(
            () => PremiumCalculator.CalculateMonthlyEquivalent(0m));
        Assert.Throws<BadRequestException>(
            () => PremiumCalculator.CalculateMonthlyEquivalent(1.001m));
    }

    [Theory]
    [InlineData("100", "900000")]
    [InlineData("20", "180000")]
    public void CoverageLimitCalculator_UsesConfiguredPercentage(
        string percentage,
        string expectedLimit)
    {
        Assert.Equal(
            decimal.Parse(expectedLimit, CultureInfo.InvariantCulture),
            CoverageLimitCalculator.Calculate(
                900000m,
                decimal.Parse(percentage, CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void PremiumCalculator_RejectsOutOfRangeRiskConfiguration()
    {
        Assert.Throws<ConflictException>(() =>
            new PremiumCalculator().Calculate(
                propertyValue: 900000m,
                riskLevel: RiskLevel.Low,
                baseRate: 0.005m,
                lowRiskMultiplier: 1.25m,
                mediumRiskMultiplier: 1m,
                highRiskMultiplier: 1.5m,
                mandatoryCoverageCharges: 425m,
                optionalCoverageCharges: 0m));
    }

    [Fact]
    public void RiskCalculator_UsesCapturedFactorsToProduceExampleLowRiskScore()
    {
        int currentYear = DateTime.UtcNow.Year;
        RiskSnapshot result = new RiskCalculator().Calculate(
            constructionYear: currentYear - 10,
            propertyValue: 900000m,
            previousClaimsCount: 1,
            disasterExposure: false,
            securitySystem: false,
            occupancyFrequency: "High",
            new RiskConfiguration
            {
                MediumPropertyValueThreshold = 500000m,
                HighPropertyValueThreshold = 1000000m,
                IsActive = true
            });

        Assert.Equal(35, result.TotalRiskScore);
        Assert.Equal(RiskLevel.Low, result.RiskLevel);
    }

    [Fact]
    public void PricingConfigurationValidator_AcceptsIllustrativeDefaults()
    {
        UpdatePricingRequest request = new()
        {
            BaseRate = 0.005m,
            LowRiskMultiplier = 1m,
            MediumRiskMultiplier = 1.25m,
            HighRiskMultiplier = 1.5m,
            RowVersion = Convert.ToBase64String([1])
        };

        var result =
            new UpdatePricingRequestValidator().Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PricingConfigurationValidator_RejectsUnorderedMultipliers()
    {
        UpdatePricingRequest request = new()
        {
            BaseRate = 0.005m,
            LowRiskMultiplier = 1.25m,
            MediumRiskMultiplier = 1m,
            HighRiskMultiplier = 1.5m,
            RowVersion = Convert.ToBase64String([1])
        };

        var result =
            new UpdatePricingRequestValidator().Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ApprovalRequest_AllowsKeepingTheCalculatedPremium()
    {
        ApproveQuoteRequest request = new()
        {
            RowVersion = Convert.ToBase64String([1])
        };

        var result =
            new ApproveQuoteRequestValidator().Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("12.345")]
    [InlineData("10000000000000000")]
    public void ApprovalRequest_RejectsInvalidRevisedPremium(
        string premiumText)
    {
        ApproveQuoteRequest request = new()
        {
            RevisedAnnualPremium =
                decimal.Parse(
                    premiumText,
                    CultureInfo.InvariantCulture),
            RowVersion = Convert.ToBase64String([1])
        };

        var result =
            new ApproveQuoteRequestValidator().Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void PaymentEligibility_RequiresAcceptanceAfterAdminApproval()
    {
        DateTime now = DateTime.UtcNow;
        Quote quote = CreateApprovedQuote(now);
        quote.CustomerAcceptedAtUtc = now.AddMinutes(-1);

        Assert.False(quote.ToDetailsDto(now).PaymentEligible);
        Assert.False(
            quote.ToValidatedQuoteDto(now).IsEligible);

        quote.CustomerAcceptedAtUtc = now.AddMinutes(1);

        Assert.True(quote.ToDetailsDto(now).PaymentEligible);
        Assert.True(
            quote.ToValidatedQuoteDto(now).IsEligible);
    }

    private static Quote CreateApprovedQuote(DateTime now)
    {
        Guid customerId = Guid.NewGuid();
        return new Quote
        {
            Id = Guid.NewGuid(),
            QuoteNumber = "QT-TEST",
            CustomerId = customerId,
            PropertyId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Status = QuoteStatus.AdminApproved,
            GeneratedAtUtc = now.AddDays(-1),
            ExpiresAtUtc = now.AddDays(7),
            PaymentDeadlineUtc = now.AddDays(2),
            AcceptedByCustomerId = customerId,
            DecidedByAdminId = Guid.NewGuid(),
            AdminDecision = AdminDecision.Approved,
            AdminDecisionAtUtc = now,
            FinalAnnualPremium = 1200m
        };
    }
}
