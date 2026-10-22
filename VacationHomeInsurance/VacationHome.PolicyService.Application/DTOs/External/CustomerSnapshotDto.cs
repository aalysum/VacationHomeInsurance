namespace VacationHome.PolicyService.Application.DTOs.External
{
    public sealed record CustomerSnapshotDto(
        Guid CustomerId,
        string FirstName,
        string LastName,
        string PhoneNumber,
        bool IsActive);
}