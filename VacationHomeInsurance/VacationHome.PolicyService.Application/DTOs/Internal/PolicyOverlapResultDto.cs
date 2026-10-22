namespace VacationHome.PolicyService.Application.DTOs.Internal
{
    public sealed record PolicyOverlapResultDto(
        bool HasConflict,
        string? ConflictCode,
        string? Message);
}
