using VacationHome.PolicyService.Application.DTOs.External;


namespace VacationHome.PolicyService.Application.DTOs.Internal
{
    public sealed record PolicyCreationRequest(
        ApprovedQuoteDto Quote,
        Guid PaymentId,
        string PolicyNumber,
        DateTime EffectiveDateUtc);
}
