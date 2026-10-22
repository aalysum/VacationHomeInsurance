using System.Net;
using System.Text.Json;
using VacationHome.ClaimService.Application.Exceptions;


namespace VacationHome.ClaimService.API.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;


        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }


        public async Task InvokeAsync(
            HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (ex is ConcurrencyConflictException)
                {
                    _logger.LogWarning(
                        ex,
                        "Claim update conflict for {Method} {Path}. Correlation ID: {CorrelationId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);
                }
                else if (ex is not BadRequestException
                    and not UnauthorizedException
                    and not ForbiddenException
                    and not NotFoundException)
                {
                    _logger.LogError(
                        ex,
                        "Unhandled exception for {Method} {Path}. Correlation ID: {CorrelationId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);
                }

                await HandleExceptionAsync(
                    context,
                    ex);
            }
        }


        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var statusCode = exception switch
            {
                BadRequestException => HttpStatusCode.BadRequest,
                UnauthorizedException => HttpStatusCode.Unauthorized,
                ForbiddenException => HttpStatusCode.Forbidden,
                NotFoundException => HttpStatusCode.NotFound,
                ConcurrencyConflictException => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError
            };


            context.Response.StatusCode = (int)statusCode;


            context.Response.ContentType = "application/json";


            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    error = statusCode == HttpStatusCode.InternalServerError
                        ? $"An unexpected server error occurred. Reference ID: {context.TraceIdentifier}"
                        : exception.Message
                }));
        }
    }
}
