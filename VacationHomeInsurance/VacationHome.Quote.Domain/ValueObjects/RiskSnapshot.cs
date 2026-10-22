using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Domain.ValueObjects
{
    public sealed record RiskSnapshot(
        int PropertyAge,
        int PropertyAgePoints,
        int PreviousClaimsPoints,
        int DisasterExposurePoints,
        int SecuritySystemPoints,
        int OccupancyPoints,
        int PropertyValuePoints,
        int TotalRiskScore,
        RiskLevel RiskLevel,
        string Explanation);
}
