using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Application.DTOs.Policies
{
    public sealed record PolicyFilterRequest
    {
        public PolicyStatus? Status { get; init; }


        public string? ProductName { get; init; }


        public int PageNumber { get; init; } = 1;


        public int PageSize { get; init; } = 10;
        public ProductType? ProductType { get; init; }
    }
}
