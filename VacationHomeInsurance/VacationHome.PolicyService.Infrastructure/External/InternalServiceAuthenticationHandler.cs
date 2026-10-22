using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace VacationHome.PolicyService.Infrastructure
    .External
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


        protected override Task<HttpResponseMessage>
            SendAsync(
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


        private string
            GenerateInternalServiceToken()
        {
            string issuer =
                _configuration[
                    "Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "Jwt:Issuer is not configured.");


            string audience =
                _configuration[
                    "Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "Jwt:Audience is not configured.");


            string secretKey =
                _configuration[
                    "Jwt:SecretKey"]
                ?? throw new InvalidOperationException(
                    "Jwt:SecretKey is not configured.");


            string serviceName =
                _configuration[
                    "InternalServiceAuthentication:" +
                    "ServiceName"]
                ?? "PolicyService";


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
                new List<Claim>
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


            var signingCredentials =
                new SigningCredentials(
                    signingKey,
                    SecurityAlgorithms.HmacSha256);


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
                        signingCredentials);


            return new JwtSecurityTokenHandler()
                .WriteToken(
                    token);
        }
    }
}
