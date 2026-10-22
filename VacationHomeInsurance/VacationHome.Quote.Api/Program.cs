using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VacationHome.ProductQuote.API.Middleware;
using VacationHome.ProductQuote.Application;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Infrastructure;
using VacationHome.ProductQuote.Infrastructure.Persistence;




var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApplication(
    builder.Configuration);


builder.Services.AddInfrastructure(
    builder.Configuration);


builder.Services
    .AddControllers()
    .AddFluentValidation();


builder.Services.AddHttpContextAccessor();


string jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Jwt:Issuer was not configured.");


string jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Jwt:Audience was not configured.");


string jwtKey =
    builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException(
        "Jwt:Key was not configured.");


builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata =
            !builder.Environment.IsDevelopment();


        options.SaveToken = false;


        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,


                ValidateAudience = true,
                ValidAudience = jwtAudience,


                ValidateLifetime = true,


                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey)),


                ClockSkew = TimeSpan.FromMinutes(1),


                NameClaimType = "sub",
                RoleClaimType = "role"
            };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        ProductQuoteAuthorizationPolicies.CustomerOnly,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Customer");
        });


    options.AddPolicy(
        ProductQuoteAuthorizationPolicies.AdminOnly,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        });


    options.AddPolicy(
        "PolicyServiceOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();


            policy.RequireClaim(
                "client_type",
                "internal_service");


            policy.RequireClaim(
                "service_name",
                "PolicyService");
        });


    options.AddPolicy(
        "ClaimsServiceOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();


            policy.RequireClaim(
                "client_type",
                "internal_service");


            policy.RequireClaim(
                "service_name",
                "ClaimsService");
        });


});


builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Vacation Home ProductQuote API",
            Version = "v1"
        });


    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter a valid JWT bearer token."
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
builder.Services.AddHealthChecks();
var app = builder.Build();


app.UseMiddleware<CorrelationIdMiddleware>();


app.UseMiddleware<ExceptionHandlingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();


    await app.Services
        .InitializeProductQuoteDatabaseAsync();
}


app.UseHttpsRedirection();


app.UseAuthentication();


app.UseAuthorization();


app.MapControllers();


app.MapHealthChecks("/health");


app.Run();
