using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.DTOs.Internal
{
    public sealed record PolicyOverlapRequestDto(
        Guid CustomerId,
        Guid PropertyId,
        ProductType ProductType,
        Guid? ExcludedQuoteId = null);
}
