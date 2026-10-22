namespace VacationHome.ClaimService.Domain.Enums
{
    public static class ClaimTypeExtensions
    {
        public static bool IsSelectableIncidentType(
            this ClaimType claimType)
        {
            return claimType is
                ClaimType.Fire or
                ClaimType.Flood or
                ClaimType.Theft or
                ClaimType.Earthquake or
                ClaimType.Wildfire or
                ClaimType.Hurricane or
                ClaimType.Other;
        }
    }
}
