using VacationHome.PolicyService.Domain.Enums;
using VacationHome.PolicyService.Application.DTOs.Documents;


namespace VacationHome.PolicyService.Application.DTOs.Payments
{
    public sealed record PaymentResultDto(
        Guid PaymentId,
        string PaymentNumber,
        PaymentStatus Status,
        Guid? PolicyId,
        string Message,
        PolicyDocumentDto? PolicyDocument = null);
}
