using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.DTOs.Payments
{
    public sealed record PaymentFilterRequest
    {
        public PaymentStatus? Status { get; init; }


        public int PageNumber { get; init; } = 1;


        public int PageSize { get; init; } = 10;
    }
}
