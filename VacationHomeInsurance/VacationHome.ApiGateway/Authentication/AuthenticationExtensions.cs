using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VacationHome.ApiGateway.Constants;


namespace VacationHome.ApiGateway.Authentication;


public static class AuthenticationExtensions
{
    public static IServiceCollection AddGatewayAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);


        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var signingKey = configuration["Jwt:SecretKey"];


        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException(
                "JWT issuer is not configured.");
        }


        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT audience is not configured.");
        }


        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured.");
        }


        if (signingKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key must contain at least 32 characters.");
        }


        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    AuthenticationSchemes.Bearer;


                options.DefaultChallengeScheme =
                    AuthenticationSchemes.Bearer;
            })
            .AddJwtBearer(
                AuthenticationSchemes.Bearer,
                options =>
                {
                    options.RequireHttpsMetadata =
                        !environment.IsDevelopment();


                    options.SaveToken = false;
                    options.MapInboundClaims = false;


                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = issuer,


                            ValidateAudience = true,
                            ValidAudience = audience,


                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(signingKey)),


                            ValidateLifetime = true,
                            RequireExpirationTime = true,
                            RequireSignedTokens = true,


                            ClockSkew = TimeSpan.FromSeconds(30),


                            NameClaimType = "sub",
                            RoleClaimType = "role"
                        };
                });


        services.AddAuthorization();


        return services;
    }
}
