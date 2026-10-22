using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VacationHome.Auth.Application.Common;
using VacationHome.Auth.Application.Exceptions;
using VacationHome.Auth.Application.Interfaces.Services;
using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Domain.Enums;


namespace VacationHome.Auth.Infrastructure.Authentication
{
    public sealed class JwtTokenService
        : IJwtTokenService
    {
        private const string TokenTypeClaim =
            "token_type";


        private const string MfaChallengeTokenType =
            "mfa_challenge";


        private const string SessionVersionClaim =
            "session_version";


        private readonly JwtSettings _settings;


        private const string
            ClaimsManagerApplicationTokenType =
                "claims_manager_application";


        private const string ApplicationStatusClaim =
            "application_status";


        public JwtTokenService(
            IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }


        public string GenerateAccessToken(
            ApplicationUser user,
            IList<string> roles)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(roles);


            var claims = new List<Claim>
    {
        new(
            JwtRegisteredClaimNames.Sub,
            user.Id),


        new(
            JwtRegisteredClaimNames.Email,
            user.Email ?? string.Empty),


        new(
            JwtRegisteredClaimNames.Jti,
            Guid.NewGuid().ToString()),


        new(
            SessionVersionClaim,
            user.RefreshTokenVersion.ToString(
                CultureInfo.InvariantCulture))
    };


            claims.AddRange(
                roles
                    .Where(
                        role =>
                            !string.IsNullOrWhiteSpace(
                                role))
                    .Select(
                        role =>
                            new Claim(
                                "role",
                                role.Trim())));


            var issuedAtUtc =
                DateTime.UtcNow;


            var token =
                new JwtSecurityToken(
                    issuer:
                        _settings.Issuer,


                    audience:
                        _settings.Audience,


                    claims:
                        claims,


                    notBefore:
                        issuedAtUtc,


                    expires:
                        issuedAtUtc.AddMinutes(
                            _settings
                                .AccessTokenExpirationMinutes),


                    signingCredentials:
                        CreateSigningCredentials());


            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }


        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));
        }


        public string GenerateMfaChallengeToken(
            ApplicationUser user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(
                    nameof(user));
            }


            var issuedAtUtc =
                DateTime.UtcNow;


            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    user.Id),


                new(
                    JwtRegisteredClaimNames.Sub,
                    user.Id),


                new(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),


                new(
                    TokenTypeClaim,
                    MfaChallengeTokenType),


                new(
                    SessionVersionClaim,
                    user.RefreshTokenVersion.ToString(
                        CultureInfo.InvariantCulture))
            };


            var token =
                new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    notBefore: issuedAtUtc,
                    expires:
                        issuedAtUtc.AddMinutes(
                            _settings
                                .MfaChallengeExpirationMinutes),
                    signingCredentials:
                        CreateSigningCredentials());


            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }


        public string ValidateMfaChallengeToken(
            string challengeToken)
        {
            if (string.IsNullOrWhiteSpace(
                    challengeToken))
            {
                throw InvalidMfaChallenge();
            }


            try
            {
                var tokenHandler =
                    new JwtSecurityTokenHandler();


                var principal =
                    tokenHandler.ValidateToken(
                        challengeToken,
                        CreateMfaValidationParameters(),
                        out var validatedToken);


                if (validatedToken is not
                    JwtSecurityToken jwtToken)
                {
                    throw new SecurityTokenException(
                        "The token is not a JWT.");
                }


                if (!string.Equals(
                        jwtToken.Header.Alg,
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.Ordinal))
                {
                    throw new SecurityTokenException(
                        "The signing algorithm is invalid.");
                }


                var tokenType =
                    principal.FindFirstValue(
                        TokenTypeClaim);


                if (!string.Equals(
                        tokenType,
                        MfaChallengeTokenType,
                        StringComparison.Ordinal))
                {
                    throw new SecurityTokenException(
                        "The token type is invalid.");
                }


                var userId =
                    principal.FindFirstValue(
                        ClaimTypes.NameIdentifier);


                if (string.IsNullOrWhiteSpace(
                        userId))
                {
                    throw new SecurityTokenException(
                        "The user identifier is missing.");
                }


                return userId;
            }
            catch (SecurityTokenException)
            {
                throw InvalidMfaChallenge();
            }
            catch (ArgumentException)
            {
                throw InvalidMfaChallenge();
            }
        }


        private TokenValidationParameters
            CreateMfaValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,


                ValidIssuer =
                    _settings.Issuer,


                ValidateAudience = true,


                ValidAudience =
                    _settings.Audience,


                ValidateIssuerSigningKey = true,


                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            _settings.SecretKey)),


                ValidateLifetime = true,


                RequireExpirationTime = true,


                RequireSignedTokens = true,


                NameClaimType =
                    ClaimTypes.NameIdentifier,


                RoleClaimType =
                    ClaimTypes.Role,


                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
        }


        private SigningCredentials
            CreateSigningCredentials()
        {
            var signingKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _settings.SecretKey));


            return new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);
        }


        private static UnauthorizedException
            InvalidMfaChallenge()
        {
            return new UnauthorizedException(
                "INVALID_MFA_CHALLENGE",
                "Invalid or expired MFA challenge.");
        }


        public string GenerateClaimsManagerApplicationToken(
            ApplicationUser user,
            ClaimsManagerApprovalStatus status)
        {
            ArgumentNullException.ThrowIfNull(user);


            var issuedAtUtc =
                DateTime.UtcNow;


            var claims = new List<Claim>
    {
        new(
            ClaimTypes.NameIdentifier,
            user.Id),


        new(
            JwtRegisteredClaimNames.Sub,
            user.Id),


        new(
            JwtRegisteredClaimNames.Jti,
            Guid.NewGuid().ToString()),


        new(
            TokenTypeClaim,
            ClaimsManagerApplicationTokenType),


        new(
            ApplicationStatusClaim,
            status.ToString()),


        new(
            "session_version",
            user.RefreshTokenVersion.ToString(
                CultureInfo.InvariantCulture))
    };


            var token =
                new JwtSecurityToken(
                    issuer:
                        _settings.Issuer,
                    audience:
                        _settings.Audience,
                    claims:
                        claims,
                    notBefore:
                        issuedAtUtc,
                    expires:
                        issuedAtUtc.AddMinutes(
                            _settings
                                .ClaimsManagerApplicationTokenExpirationMinutes),
                    signingCredentials:
                        CreateSigningCredentials());


            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }


        public string ValidateClaimsManagerApplicationToken(
            string applicationToken)
        {
            if (string.IsNullOrWhiteSpace(
                    applicationToken))
            {
                throw InvalidApplicationToken();
            }


            try
            {
                var tokenHandler =
                    new JwtSecurityTokenHandler();


                var principal =
                    tokenHandler.ValidateToken(
                        applicationToken,
                        CreateRestrictedTokenValidationParameters(),
                        out var validatedToken);


                if (validatedToken is not
                    JwtSecurityToken jwtToken)
                {
                    throw new SecurityTokenException(
                        "Invalid token.");
                }


                if (!string.Equals(
                        jwtToken.Header.Alg,
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.Ordinal))
                {
                    throw new SecurityTokenException(
                        "Invalid signing algorithm.");
                }


                var tokenType =
                    principal.FindFirstValue(
                        TokenTypeClaim);


                if (!string.Equals(
                        tokenType,
                        ClaimsManagerApplicationTokenType,
                        StringComparison.Ordinal))
                {
                    throw new SecurityTokenException(
                        "Invalid token type.");
                }


                var userId =
                    principal.FindFirstValue(
                        ClaimTypes.NameIdentifier);


                if (string.IsNullOrWhiteSpace(
                        userId))
                {
                    throw new SecurityTokenException(
                        "User identifier is missing.");
                }


                return userId;
            }
            catch (SecurityTokenException)
            {
                throw InvalidApplicationToken();
            }
            catch (ArgumentException)
            {
                throw InvalidApplicationToken();
            }
        }


        private TokenValidationParameters
            CreateRestrictedTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer =
                    _settings.Issuer,


                ValidateAudience = true,
                ValidAudience =
                    _settings.Audience,


                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            _settings.SecretKey)),


                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,


                NameClaimType =
                    ClaimTypes.NameIdentifier,


                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
        }


        private static UnauthorizedException
            InvalidApplicationToken()
        {
            return new UnauthorizedException(
                "INVALID_CLAIMS_MANAGER_APPLICATION_TOKEN",
                "Invalid or expired Claims Manager application token.");
        }
    }
}
