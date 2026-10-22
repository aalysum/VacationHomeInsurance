using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using VacationHome.ApiGateway.Authentication;
using VacationHome.ApiGateway.Configuration;
using VacationHome.ApiGateway.Middleware;
using Ocelot.Provider.Polly;


var builder = WebApplication.CreateBuilder(args);


const string FrontendCorsPolicy =
    "FrontendCorsPolicy";


builder.Configuration.AddJsonFile(
    "ocelot.json",
    optional: false,
    reloadOnChange: true);


builder.Services.Configure<GatewayOptions>(
    builder.Configuration.GetSection(
        GatewayOptions.SectionName));


var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();


if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "At least one CORS allowed origin must be configured.");
}


builder.Services.AddCors(options =>
{
    options.AddPolicy(
        FrontendCorsPolicy,
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders(
                    "Content-Disposition",
                    "X-Correlation-ID");
        });
});


builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Vacation Home API Gateway",
            Version = "v1"
        });


    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
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


builder.Services.AddGatewayAuthentication(
    builder.Configuration,
    builder.Environment);


builder.Services.AddAuthorization();


builder.Services.AddOcelot(
    builder.Configuration).AddPolly();


var app = builder.Build();


app.UseMiddleware<CorrelationIdMiddleware>();


app.UseMiddleware<GatewayExceptionMiddleware>();


app.UseSwagger();


app.UseSwaggerUI();


app.UseHttpsRedirection();


app.UseRouting();


app.UseCors(FrontendCorsPolicy);


app.UseAuthentication();


app.UseAuthorization();


app.MapControllers();


await app.UseOcelot();


app.Run();
