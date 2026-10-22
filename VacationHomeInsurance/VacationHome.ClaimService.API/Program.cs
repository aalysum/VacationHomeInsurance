using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VacationHome.ClaimService.API.Middleware;
using VacationHome.ClaimService.Application;
using VacationHome.ClaimService.Infrastructure;
using VacationHome.ClaimService.Infrastructure.Persistence;
using VacationHome.ClaimService.Application.Configuration;


var builder = WebApplication.CreateBuilder(args);


/*
 * Application dependencies.
 */
builder.Services.AddApplication();


builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.Configure<ClaimCoverageEligibilityOptions>(
    builder.Configuration.GetSection(
        ClaimCoverageEligibilityOptions.SectionName));


/*
 * API services.
 */
builder.Services.AddControllers();
builder.Services.AddHealthChecks();


builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title =
                "Vacation Home Claims Service API",
            Version = "v1",
            Description =
                "Claims management endpoints for the Vacation Home Insurance system."
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


/*
 * JWT configuration.
 */
string issuer =
    builder.Configuration[
        "JwtSettings:Issuer"]
    ?? throw new InvalidOperationException(
        "JwtSettings:Issuer is not configured.");


string audience =
    builder.Configuration[
        "JwtSettings:Audience"]
    ?? throw new InvalidOperationException(
        "JwtSettings:Audience is not configured.");


string secretKey =
    builder.Configuration[
        "JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException(
        "JwtSettings:SecretKey is not configured.");


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
        })
    .AddJwtBearer(options =>
    {
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
                        Encoding.UTF8.GetBytes(
                            secretKey)),


                ValidateLifetime = true,


                RequireExpirationTime = true,
                RequireSignedTokens = true,


                NameClaimType = "sub",
                RoleClaimType = "role",


                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
    });


/*
 * Authorization policies.
 */
builder.Services.AddAuthorization(options =>
{
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


var app = builder.Build();


/*
 * Exception and correlation middleware should run
 * before the rest of the request pipeline.
 */
app.UseMiddleware<CorrelationIdMiddleware>();


app.UseMiddleware<
    ExceptionHandlingMiddleware>();


/*
 * Enable Swagger only during development.
 */
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();


    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Vacation Home Claims Service API v1");


        options.RoutePrefix = "swagger";


        options.DocumentTitle =
            "Vacation Home Claims Service API";
    });
}


app.UseHttpsRedirection();


app.UseRouting();


app.UseAuthentication();


app.UseAuthorization();


app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ClaimDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
    await dbContext.Database.ExecuteSqlRawAsync(
        "IF COL_LENGTH('dbo.Claims', 'PolicyNumber') IS NULL " +
        "ALTER TABLE dbo.Claims ADD PolicyNumber nvarchar(50) " +
        "NOT NULL CONSTRAINT DF_Claims_PolicyNumber DEFAULT ('') WITH VALUES;");
}

app.MapHealthChecks("/health");


app.Run();
