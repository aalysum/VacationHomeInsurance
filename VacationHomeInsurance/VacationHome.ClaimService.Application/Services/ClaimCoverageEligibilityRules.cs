using VacationHome.ClaimService.Application.Configuration;
using VacationHome.ClaimService.Domain.Enums;

namespace VacationHome.ClaimService.Application.Services
{
    public static class ClaimCoverageEligibilityRules
    {
        public static bool IsConfigured(
            ClaimCoverageEligibilityOptions options,
            ProductType productType,
            ClaimType incidentType)
        {
            return options.Rules.Any(
                rule =>
                    rule.ProductType == productType &&
                    rule.IncidentType == incidentType);
        }

        public static IReadOnlyCollection<Guid> GetEligibleCoverageIds(
            ClaimCoverageEligibilityOptions options,
            ProductType productType,
            ClaimType incidentType,
            IEnumerable<Guid> policyCoverageIds)
        {
            ClaimCoverageEligibilityRule? rule =
                options.Rules.SingleOrDefault(
                    item =>
                        item.ProductType == productType &&
                        item.IncidentType == incidentType);

            if (rule is null)
            {
                return Array.Empty<Guid>();
            }

            HashSet<Guid> policyCoverageIdSet =
                policyCoverageIds.ToHashSet();

            IEnumerable<Guid> configuredCoverageIds =
                rule.UsePolicyCoverages
                    ? policyCoverageIdSet
                    : rule.CoverageIds;

            return configuredCoverageIds
                .Where(policyCoverageIdSet.Contains)
                .Distinct()
                .ToArray();
        }

        public static bool IsEligible(
            ClaimCoverageEligibilityOptions options,
            ProductType productType,
            ClaimType incidentType,
            Guid coverageId,
            IEnumerable<Guid> policyCoverageIds)
        {
            return GetEligibleCoverageIds(
                    options,
                    productType,
                    incidentType,
                    policyCoverageIds)
                .Contains(coverageId);
        }
    }
}
