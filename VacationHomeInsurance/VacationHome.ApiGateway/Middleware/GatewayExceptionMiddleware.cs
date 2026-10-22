using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace VacationHome.ApiGateway.Middleware;


public sealed class GatewayExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GatewayExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;


    public GatewayExceptionMiddleware(
        RequestDelegate next,
        ILogger<GatewayExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);


        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "The client cancelled the request. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
        catch (BadHttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "The gateway received an invalid request. TraceId: {TraceId}",
                context.TraceIdentifier);


            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Invalid request",
                "The request could not be processed.",
                "GATEWAY_INVALID_REQUEST");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An unexpected gateway error occurred. TraceId: {TraceId}",
                context.TraceIdentifier);


            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Gateway error",
                _environment.IsDevelopment()
                    ? exception.Message
                    : "The gateway could not complete the request.",
                "GATEWAY_UNEXPECTED_ERROR");
        }
    }


    private static Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string errorCode)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }


        context.Response.Clear();
        context.Response.StatusCode = statusCode;


        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };


        problemDetails.Extensions["code"] = errorCode;
        problemDetails.Extensions["traceId"] =
            context.TraceIdentifier;


        context.Response.ContentType = "application/problem+json";


        return context.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken: context.RequestAborted);
    }
}
