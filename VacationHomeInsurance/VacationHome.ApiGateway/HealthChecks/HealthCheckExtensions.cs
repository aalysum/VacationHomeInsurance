using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;


namespace VacationHome.ApiGateway.HealthChecks;


public static class HealthCheckExtensions
{
    private const string ReadinessTag = "ready";


    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);


    public static IServiceCollection AddGatewayHealthChecks(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);


        services
            .AddHealthChecks()
            .AddCheck(
                "gateway-self",
                () => HealthCheckResult.Healthy(
                    "The API Gateway is running."),
                tags: [ReadinessTag]);


        return services;
    }


    public static IEndpointRouteBuilder MapGatewayHealthChecks(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);


        endpoints.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = _ => false
            });


        endpoints.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = registration =>
                    registration.Tags.Contains(ReadinessTag),
                ResponseWriter = WriteHealthResponseAsync
            });


        return endpoints;
    }


    private static Task WriteHealthResponseAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";


        var response = new
        {
            status = report.Status.ToString(),
            durationMilliseconds =
                report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMilliseconds =
                    entry.Value.Duration.TotalMilliseconds
            })
        };


        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, SerializerOptions),
            context.RequestAborted);
    }
}
