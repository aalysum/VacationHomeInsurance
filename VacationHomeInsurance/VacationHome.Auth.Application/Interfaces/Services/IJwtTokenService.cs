using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Domain.Enums;


namespace VacationHome.Auth.Application.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(
            ApplicationUser user,
            IList<string> roles);


        string GenerateRefreshToken();


        string GenerateMfaChallengeToken(
            ApplicationUser user);


        string ValidateMfaChallengeToken(
            string challengeToken);


        string GenerateClaimsManagerApplicationToken(
            ApplicationUser user,
            ClaimsManagerApprovalStatus status);


        string ValidateClaimsManagerApplicationToken(
            string applicationToken);
    }
}
