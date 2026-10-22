using VacationHome.PolicyService.Domain.Enums;
using VacationHome.PolicyService.Application.DTOs.Documents;


namespace VacationHome.PolicyService.Application.DTOs.Policies
{
    public sealed record PolicySummaryDto(
        Guid Id,
        string PolicyNumber,


        Guid CustomerId,


        Guid PropertyId,
        string PropertyName,


        ProductType ProductType,
        string ProductName,


        decimal AnnualPremium,
        DateTime EffectiveDateUtc,
        DateTime ExpiryDateUtc,
        PolicyStatus Status,
        IReadOnlyCollection<PolicyDocumentDto> Documents);
}
