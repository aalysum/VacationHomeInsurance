using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VacationHome.Auth.Application.Common;
using VacationHome.Auth.Application.Interfaces.Services;


namespace VacationHome.Auth.Infrastructure.Security
{
    public class InternalTokenService
        : IInternalTokenService
    {
        private readonly JwtSettings _jwtSettings;


        public InternalTokenService(
            IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings =
                jwtOptions.Value;
        }


        public string GenerateAuthServiceToken()
        {
            return GenerateToken(
                serviceName:
                    InternalServiceNames.AuthService,
                subject:
                    "auth-service");
        }


        public string GenerateQuoteServiceToken()
        {
            return GenerateToken(
                serviceName:
                    InternalServiceNames.QuoteService,
                subject:
                    "quote-service");
        }


        private string GenerateToken(
            string serviceName,
            string subject)
        {
            ValidateSettings();


            var utcNow =
                DateTime.UtcNow;


            var claims =
                new List<Claim>
                {
                    new(
                        JwtRegisteredClaimNames.Sub,
                        subject),


                    new(
                        ClaimTypes.NameIdentifier,
                        subject),


                    new(
                        "role",
                        InternalTokenClaims
                            .InternalServiceRole),


                    new(
                        InternalTokenClaims.ClientType,
                        InternalTokenClaims
                            .InternalService),


                    new(
                        InternalTokenClaims.ServiceName,
                        serviceName),


                    new(
                        JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString())
                };


            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _jwtSettings.SecretKey));


            var signingCredentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256);


            var token =
                new JwtSecurityToken(
                    issuer:
                        _jwtSettings.Issuer,


                    audience:
                        _jwtSettings.Audience,


                    claims:
                        claims,


                    notBefore:
                        utcNow,


                    expires:
                        utcNow.AddMinutes(
                            _jwtSettings
                                .InternalTokenExpirationMinutes),


                    signingCredentials:
                        signingCredentials);


            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }


        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(
                    _jwtSettings.Issuer))
            {
                throw new InvalidOperationException(
                    "JWT issuer is not configured.");
            }


            if (string.IsNullOrWhiteSpace(
                    _jwtSettings.Audience))
            {
                throw new InvalidOperationException(
                    "JWT audience is not configured.");
            }


            if (string.IsNullOrWhiteSpace(
                    _jwtSettings.SecretKey) ||
                _jwtSettings.SecretKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT secret key must contain at least 32 characters.");
            }


            if (_jwtSettings.InternalTokenExpirationMinutes <= 0)
            {
                throw new InvalidOperationException(
                    "Internal token lifetime must be positive.");
            }
        }
    }
}
