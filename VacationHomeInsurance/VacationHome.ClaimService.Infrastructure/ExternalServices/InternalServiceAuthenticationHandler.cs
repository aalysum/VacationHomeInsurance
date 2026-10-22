using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace VacationHome.ClaimService.Infrastructure
    .ExternalServices
{
    public sealed class
        InternalServiceAuthenticationHandler
        : DelegatingHandler
    {
        private readonly IConfiguration
            _configuration;


        public InternalServiceAuthenticationHandler(
            IConfiguration configuration)
        {
            _configuration =
                configuration;
        }


        protected override Task<
            HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            string token =
                GenerateInternalServiceToken();


            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);


            return base.SendAsync(
                request,
                cancellationToken);
        }


        private string GenerateInternalServiceToken()
        {
            string issuer =
                _configuration[
                    "JwtSettings:Issuer"]
                ?? throw new InvalidOperationException(
                    "JwtSettings:Issuer is not configured.");


            string audience =
                _configuration[
                    "JwtSettings:Audience"]
                ?? throw new InvalidOperationException(
                    "JwtSettings:Audience is not configured.");


            string secretKey =
                _configuration[
                    "JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException(
                    "JwtSettings:SecretKey is not configured.");


            string serviceName =
                _configuration[
                    "InternalServiceAuthentication:" +
                    "ServiceName"]
                ?? "ClaimService";


            int tokenLifetimeMinutes =
                _configuration.GetValue(
                    "InternalServiceAuthentication:" +
                    "TokenLifetimeMinutes",
                    5);


            if (tokenLifetimeMinutes < 1)
            {
                tokenLifetimeMinutes =
                    5;
            }


            DateTime issuedAtUtc =
                DateTime.UtcNow;


            var claims =
                new List<System.Security.Claims.Claim>
                {
                    new(
                        JwtRegisteredClaimNames.Sub,
                        serviceName),


                    new(
                        JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString("N")),


                    new(
                        "client_type",
                        "internal_service"),


                    new(
                        "client_id",
                        serviceName),

                    new(
                        "service_name",
                        serviceName)
                };


            var signingKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        secretKey));


            var credentials =
                new SigningCredentials(
                    signingKey,
                    SecurityAlgorithms
                        .HmacSha256);


            var token =
                new JwtSecurityToken(
                    issuer:
                        issuer,
                    audience:
                        audience,
                    claims:
                        claims,
                    notBefore:
                        issuedAtUtc,
                    expires:
                        issuedAtUtc.AddMinutes(
                            tokenLifetimeMinutes),
                    signingCredentials:
                        credentials);


            return new JwtSecurityTokenHandler()
                .WriteToken(
                    token);
        }
    }
}
