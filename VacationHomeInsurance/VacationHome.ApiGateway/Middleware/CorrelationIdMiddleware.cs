using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using VacationHome.ApiGateway.Configuration;
using VacationHome.ApiGateway.Constants;


namespace VacationHome.ApiGateway.Middleware;


public sealed partial class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private readonly GatewayOptions _options;


    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger,
        IOptions<GatewayOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);


        var correlationId = GetCorrelationId(context.Request);


        context.TraceIdentifier = correlationId;


        context.Request.Headers[GatewayHeaders.CorrelationId] =
            correlationId;


        context.Response.OnStarting(() =>
        {
            context.Response.Headers[GatewayHeaders.CorrelationId] =
                correlationId;


            return Task.CompletedTask;
        });


        Activity.Current?.SetTag(
            "correlation.id",
            correlationId);


        using (_logger.BeginScope(
                   new Dictionary<string, object>
                   {
                       ["CorrelationId"] = correlationId
                   }))
        {
            await _next(context);
        }
    }


    private string GetCorrelationId(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(
                GatewayHeaders.CorrelationId,
                out var headerValues))
        {
            return CreateCorrelationId();
        }


        var suppliedValue = headerValues.FirstOrDefault();


        if (string.IsNullOrWhiteSpace(suppliedValue))
        {
            return CreateCorrelationId();
        }


        suppliedValue = suppliedValue.Trim();


        if (suppliedValue.Length >
            _options.MaximumCorrelationIdLength)
        {
            _logger.LogWarning(
                "The supplied correlation ID exceeded the allowed length.");


            return CreateCorrelationId();
        }


        if (!CorrelationIdPattern().IsMatch(suppliedValue))
        {
            _logger.LogWarning(
                "The supplied correlation ID contained invalid characters.");


            return CreateCorrelationId();
        }


        return suppliedValue;
    }


    private static string CreateCorrelationId()
    {
        return Guid.NewGuid().ToString("N");
    }


    [GeneratedRegex( "^[A-Za-z0-9._:-]+$",RegexOptions.CultureInvariant,matchTimeoutMilliseconds: 100)]
    private static partial Regex CorrelationIdPattern();
}
