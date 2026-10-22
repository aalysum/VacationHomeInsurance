using System.Text.Json;
using System.Text.Json.Serialization;
using VacationHome.ClaimService.Application.Configuration;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Application.Services;
using VacationHome.ClaimService.Application.Validators;
using VacationHome.ClaimService.Domain.Enums;
using Xunit;

namespace VacationHome.ClaimService.Tests;

public sealed class ClaimCoverageEligibilityRulesTests
{
    private static readonly Guid DwellingCoverageId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    private static readonly Guid OtherStructuresCoverageId =
        Guid.Parse("10000000-0000-0000-0000-000000000002");

    private static readonly Guid PersonalPropertyCoverageId =
        Guid.Parse("10000000-0000-0000-0000-000000000003");

    private static readonly Guid TheftProtectionCoverageId =
        Guid.Parse("10000000-0000-0000-0000-000000000004");

    private static readonly Guid FloodCoverageId =
        Guid.Parse("10000000-0000-0000-0000-000000000005");

    private static readonly Guid EarthquakeCoverageId =
        Guid.Parse("10000000-0000-0000-0000-000000000006");

    private static readonly Guid WildfireCoverageId =
        Guid.Parse("10000000-0000-0000-0000-000000000007");

    private static readonly Guid HurricaneCoverageId =
        Guid.Parse("10000000-0000-0000-0000-000000000008");

    [Fact]
    public void ConfiguredRules_MatchAllRequestedPlanIncidentMappings()
    {
        ClaimCoverageEligibilityOptions options =
            LoadConfiguredOptions();

        AssertMapping(
            options,
            ProductType.ComprehensiveProtection,
            ClaimType.Fire,
            DwellingCoverageId,
            OtherStructuresCoverageId,
            PersonalPropertyCoverageId);
        AssertMapping(
            options,
            ProductType.ComprehensiveProtection,
            ClaimType.Flood,
            FloodCoverageId);
        AssertMapping(
            options,
            ProductType.ComprehensiveProtection,
            ClaimType.Theft,
            TheftProtectionCoverageId,
            PersonalPropertyCoverageId);
        AssertMapping(
            options,
            ProductType.ComprehensiveProtection,
            ClaimType.Earthquake,
            EarthquakeCoverageId);
        AssertMapping(
            options,
            ProductType.ComprehensiveProtection,
            ClaimType.Wildfire,
            WildfireCoverageId);
        AssertMapping(
            options,
            ProductType.ComprehensiveProtection,
            ClaimType.Hurricane,
            HurricaneCoverageId);
        AssertPolicyCoverageRule(
            options,
            ProductType.ComprehensiveProtection,
            ClaimType.Other);

        AssertMapping(
            options,
            ProductType.NaturalDisasterProtection,
            ClaimType.Flood,
            FloodCoverageId);
        AssertMapping(
            options,
            ProductType.NaturalDisasterProtection,
            ClaimType.Earthquake,
            EarthquakeCoverageId);
        AssertMapping(
            options,
            ProductType.NaturalDisasterProtection,
            ClaimType.Wildfire,
            WildfireCoverageId);
        AssertMapping(
            options,
            ProductType.NaturalDisasterProtection,
            ClaimType.Hurricane,
            HurricaneCoverageId);
        AssertMapping(
            options,
            ProductType.NaturalDisasterProtection,
            ClaimType.Fire);
        AssertMapping(
            options,
            ProductType.NaturalDisasterProtection,
            ClaimType.Theft);
        AssertPolicyCoverageRule(
            options,
            ProductType.NaturalDisasterProtection,
            ClaimType.Other);

        AssertMapping(
            options,
            ProductType.PropertyProtection,
            ClaimType.Fire,
            DwellingCoverageId,
            OtherStructuresCoverageId,
            PersonalPropertyCoverageId);
        AssertMapping(
            options,
            ProductType.PropertyProtection,
            ClaimType.Theft,
            TheftProtectionCoverageId,
            PersonalPropertyCoverageId);
        AssertMapping(
            options,
            ProductType.PropertyProtection,
            ClaimType.Flood);
        AssertMapping(
            options,
            ProductType.PropertyProtection,
            ClaimType.Earthquake);
        AssertMapping(
            options,
            ProductType.PropertyProtection,
            ClaimType.Wildfire);
        AssertMapping(
            options,
            ProductType.PropertyProtection,
            ClaimType.Hurricane);
        AssertPolicyCoverageRule(
            options,
            ProductType.PropertyProtection,
            ClaimType.Other);
    }

    [Fact]
    public void EmptyConfiguredRule_IsConfiguredButAllowsNoCoverages()
    {
        var options = new ClaimCoverageEligibilityOptions
        {
            Rules =
            [
                new ClaimCoverageEligibilityRule
                {
                    ProductType = ProductType.PropertyProtection,
                    IncidentType = ClaimType.Flood
                }
            ]
        };

        Assert.True(
            ClaimCoverageEligibilityRules.IsConfigured(
                options,
                ProductType.PropertyProtection,
                ClaimType.Flood));
        Assert.Empty(
            ClaimCoverageEligibilityRules.GetEligibleCoverageIds(
                options,
                ProductType.PropertyProtection,
                ClaimType.Flood,
                [DwellingCoverageId]));
    }

    [Fact]
    public void MissingRule_IsNotConfigured()
    {
        var options = new ClaimCoverageEligibilityOptions();

        Assert.False(
            ClaimCoverageEligibilityRules.IsConfigured(
                options,
                ProductType.PropertyProtection,
                ClaimType.Flood));
    }

    [Fact]
    public void Other_ReturnsAllAndOnlyCoveragesFromSelectedPolicy()
    {
        ClaimCoverageEligibilityOptions options =
            LoadConfiguredOptions();
        Guid[] policyCoverageIds =
        [
            DwellingCoverageId,
            PersonalPropertyCoverageId
        ];

        IReadOnlyCollection<Guid> eligibleCoverageIds =
            ClaimCoverageEligibilityRules.GetEligibleCoverageIds(
                options,
                ProductType.PropertyProtection,
                ClaimType.Other,
                policyCoverageIds);

        Assert.Equal(policyCoverageIds, eligibleCoverageIds);
        Assert.DoesNotContain(
            TheftProtectionCoverageId,
            eligibleCoverageIds);
    }

    [Fact]
    public void OptionalCoverageNotOnPolicy_IsNotEligible()
    {
        ClaimCoverageEligibilityOptions options =
            LoadConfiguredOptions();

        Assert.False(
            ClaimCoverageEligibilityRules.IsEligible(
                options,
                ProductType.PropertyProtection,
                ClaimType.Theft,
                TheftProtectionCoverageId,
                [PersonalPropertyCoverageId]));
    }

    [Fact]
    public void ExplicitRule_AllowsOnlyConfiguredCoverageIdsOnPolicy()
    {
        ClaimCoverageEligibilityOptions options =
            LoadConfiguredOptions();

        Assert.True(
            ClaimCoverageEligibilityRules.IsEligible(
                options,
                ProductType.PropertyProtection,
                ClaimType.Fire,
                DwellingCoverageId,
                [DwellingCoverageId]));
        Assert.False(
            ClaimCoverageEligibilityRules.IsEligible(
                options,
                ProductType.PropertyProtection,
                ClaimType.Fire,
                FloodCoverageId,
                [DwellingCoverageId, FloodCoverageId]));
    }

    [Fact]
    public void ClaimRequestValidator_AllowsMultipleCoveragesWithSeparateAmounts()
    {
        var request = new CreateClaimRequest
        {
            PolicyId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            Type = ClaimType.Fire,
            IncidentDate = DateTime.UtcNow.Date,
            Title = "Fire damage",
            Description = "Damage to the dwelling and belongings.",
            Coverages =
            [
                new ClaimCoverageRequest
                {
                    CoverageId = DwellingCoverageId,
                    RequestedAmount = 1200m
                },
                new ClaimCoverageRequest
                {
                    CoverageId = PersonalPropertyCoverageId,
                    RequestedAmount = 300m
                }
            ]
        };

        var result =
            new CreateClaimRequestValidator()
                .Validate(request);

        Assert.True(result.IsValid);
        Assert.Equal(2, request.Coverages.Count);
        Assert.Equal(
            [1200m, 300m],
            request.Coverages
                .Select(coverage => coverage.RequestedAmount));
    }

    [Fact]
    public void LegacyDamageCategories_AreNotSelectableIncidentTypes()
    {
        Assert.False(
            ClaimType.DwellingDamage.IsSelectableIncidentType());
        Assert.True(
            ClaimType.Fire.IsSelectableIncidentType());
    }

    private static ClaimCoverageEligibilityOptions LoadConfiguredOptions()
    {
        string settingsPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "appsettings.json");
        string settingsJson =
            File.ReadAllText(settingsPath);
        using JsonDocument document =
            JsonDocument.Parse(settingsJson);
        string eligibilityJson =
            document.RootElement
                .GetProperty("ClaimCoverageEligibility")
                .GetRawText();

        return JsonSerializer.Deserialize<
                ClaimCoverageEligibilityOptions>(
                eligibilityJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters =
                    {
                        new JsonStringEnumConverter()
                    }
                })
            ?? throw new InvalidOperationException(
                "Claim coverage eligibility configuration is missing.");
    }

    private static void AssertMapping(
        ClaimCoverageEligibilityOptions options,
        ProductType productType,
        ClaimType incidentType,
        params Guid[] expectedCoverageIds)
    {
        Assert.True(
            ClaimCoverageEligibilityRules.IsConfigured(
                options,
                productType,
                incidentType));

        ClaimCoverageEligibilityRule rule =
            options.Rules.Single(
                item =>
                    item.ProductType == productType &&
                    item.IncidentType == incidentType);

        Assert.Equal(
            expectedCoverageIds.Order(),
            rule.CoverageIds.Order());

        IReadOnlyCollection<Guid> actualCoverageIds =
            ClaimCoverageEligibilityRules.GetEligibleCoverageIds(
                options,
                productType,
                incidentType,
                expectedCoverageIds);

        Assert.Equal(
            expectedCoverageIds.Order(),
            actualCoverageIds.Order());

        Assert.False(rule.UsePolicyCoverages);
    }

    private static void AssertPolicyCoverageRule(
        ClaimCoverageEligibilityOptions options,
        ProductType productType,
        ClaimType incidentType)
    {
        Assert.True(
            ClaimCoverageEligibilityRules.IsConfigured(
                options,
                productType,
                incidentType));

        ClaimCoverageEligibilityRule rule =
            options.Rules.Single(
                item =>
                    item.ProductType == productType &&
                    item.IncidentType == incidentType);

        Assert.True(rule.UsePolicyCoverages);
        Assert.Empty(rule.CoverageIds);
    }
}
