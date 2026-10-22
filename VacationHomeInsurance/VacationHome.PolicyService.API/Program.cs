using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using VacationHome.PolicyService.API.Middleware;
using VacationHome.PolicyService.Application;
using VacationHome.PolicyService.Infrastructure;
using VacationHome.PolicyService.Infrastructure.Persistence;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApplication();


builder.Services.AddInfrastructure(
    builder.Configuration);


builder.Services
    .AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title =
                "Vacation Home Policy Service API",


            Version =
                "v1"
        });


    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name =
                "Authorization",


            Type =
                SecuritySchemeType.Http,


            Scheme =
                "bearer",


            BearerFormat =
                "JWT",


            In =
                ParameterLocation.Header,


            Description =
                "Enter a valid JWT access token."
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


                            Id =
                                "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});


string issuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException();


string audience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException();


string secretKey =
    builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException();


builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
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


                NameClaimType = "sub",


                RoleClaimType = "role",


                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
    });


builder.Services.AddAuthorization(
    options =>
    {


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


app.UseSwagger();


app.UseSwaggerUI();
app.UseMiddleware<
    CorrelationIdMiddleware>();


app.UseMiddleware<ExceptionHandlingMiddleware>();


app.UseAuthentication();


app.UseAuthorization();


app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<PolicyDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
    await PolicyDocumentSchemaMigration.ApplyAsync(
        dbContext,
        CancellationToken.None);
}

app.MapHealthChecks("/health");


app.Run();
