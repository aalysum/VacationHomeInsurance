namespace VacationHome.PolicyService.Application.DTOs.Internal
{
    public sealed record PolicyCreationResult(
        Guid PolicyId,
        string PolicyNumber);
}
