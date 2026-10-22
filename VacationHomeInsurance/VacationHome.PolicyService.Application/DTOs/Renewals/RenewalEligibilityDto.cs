namespace VacationHome.PolicyService.Application.DTOs.Renewals
{
    public sealed record RenewalEligibilityDto(
        bool Eligible,
        string Reason);
}
