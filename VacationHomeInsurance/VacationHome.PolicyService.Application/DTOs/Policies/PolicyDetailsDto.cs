using VacationHome.PolicyService.Domain.Enums;
using VacationHome.PolicyService.Application.DTOs.Documents;


namespace VacationHome.PolicyService.Application.DTOs.Policies
{
    public sealed record PolicyDetailsDto(
        Guid Id,
        string PolicyNumber,


        Guid QuoteId,


        Guid CustomerId,


        Guid PropertyId,
        string PropertyName,
        string PropertyAddressLine1,
        string? PropertyAddressLine2,
        string PropertyCity,
        string PropertyState,
        string PropertyPostalCode,
        string PropertyCountry,
        string PropertyType,
        decimal PropertyValue,
        int ConstructionYear,
        string OccupancyFrequency,
        bool HasSecuritySystem,

        ProductType ProductType,
        string ProductName,


        decimal AnnualPremium,
        DateTime EffectiveDateUtc,
        DateTime ExpiryDateUtc,
        PolicyStatus Status,


        IReadOnlyCollection<PolicyCoverageDto> Coverages,
        IReadOnlyCollection<PolicyDocumentDto> Documents);
}
