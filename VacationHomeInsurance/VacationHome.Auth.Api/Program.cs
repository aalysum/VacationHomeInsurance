using System.Security.Claims;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VacationHome.Auth.API.Extensions;
using VacationHome.Auth.API.Middleware;
using VacationHome.Auth.Application;
using VacationHome.Auth.Application.Common;
using VacationHome.Auth.Application.Interfaces.Services;
using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Domain.Enums;
using VacationHome.Auth.Infrastructure;
using VacationHome.Auth.Infrastructure.ExternalServices;
using VacationHome.Auth.Infrastructure.Identity;
using VacationHome.Auth.Infrastructure.Persistence.Context;
using System.IdentityModel.Tokens.Jwt;


var builder =
    WebApplication.CreateBuilder(args);


/*
 * Options and configuration
 */
builder.Services
    .AddOptions<JwtSettings>()
    .Bind(
        builder.Configuration
            .GetSection("Jwt"))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.Issuer),
        "JWT issuer is required.")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.Audience),
        "JWT audience is required.")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.SecretKey) &&
            settings.SecretKey.Length >= 32,
        "JWT secret key must contain at least 32 characters.")
    .Validate(
        settings =>
            settings
                .AccessTokenExpirationMinutes > 0,
        "Access-token expiration must be greater than zero.")
    .Validate(
        settings =>
            settings
                .RefreshTokenExpirationDays > 0,
        "Refresh-token expiration must be greater than zero.")
    .Validate(
        settings =>
            settings
                .MfaChallengeExpirationMinutes > 0,
            "MFA challenge expiration must be greater than zero.")
    .Validate(
        settings =>
            settings
                .ClaimsManagerApplicationTokenExpirationMinutes > 0,
        "Claims Manager application-token expiration must be greater than zero.")
    .ValidateOnStart();


builder.Services
    .AddOptions<FileStorageSettings>()
    .Bind(
        builder.Configuration
            .GetSection("FileStorage"))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings
                    .ClaimsManagerDocumentsPath),
        "Claims Manager document path is required.")
    .ValidateOnStart();


/*
 * Application and Infrastructure layers
 */
builder.Services.AddApplication();


builder.Services.AddInfrastructure(
    builder.Configuration);


/*
 * Development-only password-reset notification.
 *
 * Replace this implementation with a real email or
 * Notification Service implementation before production.
 */
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<
        IPasswordResetNotificationService,
        DevelopmentPasswordResetNotificationService>();
}
else
{
    throw new InvalidOperationException(
        "A production IPasswordResetNotificationService " +
        "implementation must be registered.");
}


/*
 * Controllers and FluentValidation
 */
builder.Services.AddControllers();
builder.Services.AddHealthChecks();


builder.Services
    .AddFluentValidationAutoValidation();


builder.Services
    .Configure<ApiBehaviorOptions>(
        options =>
        {
            options.InvalidModelStateResponseFactory =
                context =>
                {
                    var errors =
                        context.ModelState
                            .Where(
                                entry =>
                                    entry.Value?
                                        .Errors.Count > 0)
                            .ToDictionary(
                                entry =>
                                    entry.Key,


                                entry =>
                                    entry.Value!
                                        .Errors
                                        .Select(
                                            error =>
                                                string
                                                    .IsNullOrWhiteSpace(
                                                        error
                                                            .ErrorMessage)
                                                    ? "Invalid value."
                                                    : error
                                                        .ErrorMessage)
                                        .ToArray());


                    return new BadRequestObjectResult(
                        new
                        {
                            success = false,
                            code =
                                "VALIDATION_FAILED",
                            message =
                                "One or more validation errors occurred.",
                            errors
                        });
                };
        });


builder.Services.AddEndpointsApiExplorer();


/*
 * Read validated JWT settings for bearer configuration.
 */
var jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");


/*
 * Authentication
 */
builder.Services
    .AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;


            options.DefaultChallengeScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;


            options.DefaultScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;
        })
    .AddJwtBearer(
        options =>
        {
            options.RequireHttpsMetadata =
                !builder.Environment
                    .IsDevelopment();


            options.SaveToken = false;


            options.MapInboundClaims = false;


            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,


                    ValidateAudience = true,


                    ValidateIssuerSigningKey = true,


                    ValidateLifetime = true,


                    RequireExpirationTime = true,


                    RequireSignedTokens = true,


                    ValidIssuer =
                        jwtSettings.Issuer,


                    ValidAudience =
                        jwtSettings.Audience,


                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtSettings
                                    .SecretKey)),


                    NameClaimType =   JwtRegisteredClaimNames.Sub,


                    RoleClaimType =  "role",


                    ClockSkew =
                        TimeSpan.FromSeconds(30)
                };


            options.Events =
                new JwtBearerEvents
                {
                    OnTokenValidated =
                        async context =>
                        {
                            await ValidateSessionAsync(
                                context);
                        },


                    OnChallenge =
                        async context =>
                        {
                            if (context.Response
                                .HasStarted)
                            {
                                return;
                            }


                            context.HandleResponse();


                            context.Response.StatusCode =
                                StatusCodes
                                    .Status401Unauthorized;


                            context.Response.ContentType =
                                "application/json";


                            await context.Response
                                .WriteAsJsonAsync(
                                    new
                                    {
                                        success = false,
                                        code =
                                            "UNAUTHORIZED",
                                        message =
                                            "Authentication is required."
                                    });
                        },


                    OnForbidden =
                        async context =>
                        {
                            if (context.Response
                                .HasStarted)
                            {
                                return;
                            }


                            context.Response.StatusCode =
                                StatusCodes
                                    .Status403Forbidden;


                            context.Response.ContentType =
                                "application/json";


                            await context.Response
                                .WriteAsJsonAsync(
                                    new
                                    {
                                        success = false,
                                        code =
                                            "FORBIDDEN",
                                        message =
                                            "You do not have permission to perform this action."
                                    });
                        }
                };
        });


/*
 * Authorization
 */
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "ClaimsServiceOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("client_type", "internal_service");
            policy.RequireClaim("service_name", "ClaimsService");
        });

    options.AddPolicy(
    "InternalServiceOnly",
    policy =>
    {
        policy.RequireAuthenticatedUser();


        policy.RequireClaim(
            "client_type",
            "internal_service");
    });
});


/*
 * Swagger
 */
builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title =
                    "Vacation Home Auth API",


                Version = "v1"
            });


        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",


                Type =
                    SecuritySchemeType.Http,


                Scheme = "bearer",


                BearerFormat = "JWT",


                In =
                    ParameterLocation.Header,


                Description =
                    "Enter the JWT access token."
            });


        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =
                            new OpenApiReference
                            {
                                Type =
                                    ReferenceType
                                        .SecurityScheme,


                                Id = "Bearer"
                            }
                    },


                    Array.Empty<string>()
                }
            });
    });


var app = builder.Build();


/*
 * Middleware pipeline
 */
app.UseMiddleware<
    CorrelationIdMiddleware>();


app.UseCustomExceptionHandling();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


/*
 * If HTTPS redirection is correctly configured in your
 * development environment, this can remain enabled.
 */
app.UseHttpsRedirection();


app.UseAuthentication();


app.UseAuthorization();


app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AuthDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
}

/*
 * Seed roles and the development administrator.
 */
await IdentitySeeder.SeedAsync(
    app.Services);
app.MapHealthChecks("/health");


app.Run();


static async Task ValidateSessionAsync(
    TokenValidatedContext context)
{
    var principal =
        context.Principal;


    if (principal is null)
    {
        context.Fail(
            "The token principal is missing.");


        return;
    }


    /*
     * MFA challenge tokens are validated manually by
     * JwtTokenService.ValidateMfaChallengeToken().
     *
     * They must never be accepted as regular bearer
     * access tokens.
     */
    var tokenType =
        principal.FindFirstValue(
            "token_type");


    if (!string.IsNullOrWhiteSpace(
            tokenType))
    {
        context.Fail(
            "The token cannot be used as an access token.");


        return;
    }


    var clientType =
    principal.FindFirstValue(
        "client_type");


    if (string.Equals(
            clientType,
            "internal_service",
            StringComparison.Ordinal))
    {
        return;
    }


    var userId =
        principal.FindFirstValue(
            JwtRegisteredClaimNames.Sub);


    var sessionVersionValue =
        principal.FindFirstValue(
            "session_version");


    if (string.IsNullOrWhiteSpace(
            userId) ||
        !int.TryParse(
            sessionVersionValue,
            out var sessionVersion))
    {
        context.Fail(
            "The access token is invalid.");


        return;
    }


    var userManager =
        context.HttpContext
            .RequestServices
            .GetRequiredService<
                UserManager<ApplicationUser>>();


    var user =
        await userManager.FindByIdAsync(
            userId);


    if (user is null)
    {
        context.Fail(
            "The user no longer exists.");


        return;
    }


    if (user.Status !=
        UserStatus.Active)
    {
        context.Fail(
            "The user account is inactive.");


        return;
    }


    if (user.RefreshTokenVersion !=
        sessionVersion)
    {
        context.Fail(
            "The session has been invalidated.");
    }
}
