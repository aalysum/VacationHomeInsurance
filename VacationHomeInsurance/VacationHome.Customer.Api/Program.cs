using System.Security.Claims;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VacationHome.Customer.API.Middleware;
using VacationHome.Customer.Application;
using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Infrastructure;
using VacationHome.Customer.Infrastructure.Persistence.Context;


var builder =
    WebApplication.CreateBuilder(args);


builder.Services.AddApplication();


builder.Services.AddInfrastructure(
    builder.Configuration);


builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.Issuer),
        "JWT issuer is required.")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.Audience),
        "JWT audience is required.")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.SecretKey),
        "JWT secret key is required.")
    .Validate(
        settings => settings.SecretKey.Length >= 32,
        "JWT secret key must contain at least 32 characters.")
    .ValidateOnStart();


var jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are not configured.");




builder.Services
    .AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;


            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;


            options.DefaultScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
    .AddJwtBearer(
        options =>
        {
            options.MapInboundClaims = false;


            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,


                    ValidIssuer =
                        jwtSettings.Issuer,


                    ValidAudience =
                        jwtSettings.Audience,


                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtSettings.SecretKey)),


                    NameClaimType = "sub",


                    RoleClaimType = "role",


                    ClockSkew =
                        TimeSpan.FromMinutes(1)
                };
        });
builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy(
            "AuthServiceOnly",
            policy =>
            {
                policy.RequireAuthenticatedUser();


                policy.RequireClaim(
                    "client_type",
                    "internal_service");


                policy.RequireClaim(
                    "service_name",
                    "AuthService");
            });


        options.AddPolicy(
            "QuoteServiceOnly",
            policy =>
            {
                policy.RequireAuthenticatedUser();


                policy.RequireClaim(
                    "client_type",
                    "internal_service");


                policy.RequireClaim(
                    "service_name",
                    "QuoteService");
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


                options.AddPolicy(
        "ProductQuoteServiceOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();


            policy.RequireClaim(
            "client_type",
            "internal_service");


            policy.RequireClaim(
            "service_name",
            "ProductQuoteService");
        });
    });


builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(
        options =>
        {
            options.InvalidModelStateResponseFactory =
                context =>
                {
                    var errors =
                        context.ModelState
                            .Where(
                                entry =>
                                    entry.Value?.Errors
                                        .Count > 0)
                            .ToDictionary(
                                entry => entry.Key,
                                entry => entry.Value!
                                    .Errors
                                    .Select(
                                        error =>
                                            string.IsNullOrWhiteSpace(
                                                error.ErrorMessage)
                                                ? "The supplied value is invalid."
                                                : error.ErrorMessage)
                                    .Distinct()
                                    .ToArray());


                    var response = new
                    {
                        type =
                            "https://httpstatuses.com/400",


                        title =
                            "Validation failed",


                        status =
                            StatusCodes
                                .Status400BadRequest,


                        code =
                            "VALIDATION_ERROR",


                        detail =
                            "One or more validation errors occurred.",


                        correlationId =
                            context.HttpContext
                                .TraceIdentifier,


                        errors
                    };


                    return new BadRequestObjectResult(
                        response)
                    {
                        ContentTypes =
                        {
                            "application/problem+json"
                        }
                    };
                };
        });


builder.Services
    .AddFluentValidationAutoValidation();


builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title =
                    "Vacation Home Customer Service",


                Version =
                    "v1"
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
                                    ReferenceType.SecurityScheme,


                                Id = "Bearer"
                            }
                    },
                    Array.Empty<string>()
                }
            });
    });


builder.Services.AddHealthChecks();


var app = builder.Build();


app.UseMiddleware<
    CorrelationIdMiddleware>();


app.UseMiddleware<
    ExceptionHandlingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();


    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "Customer Service v1");
        });
}


app.UseHttpsRedirection();


app.UseAuthentication();


app.UseAuthorization();


app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<CustomerDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
}

app.MapHealthChecks("/health");


app.Run();
