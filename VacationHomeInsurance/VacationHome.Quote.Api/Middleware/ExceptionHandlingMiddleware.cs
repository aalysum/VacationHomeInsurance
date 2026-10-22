using Microsoft.EntityFrameworkCore;
using VacationHome.ProductQuote.Application.Exceptions;


namespace VacationHome.ProductQuote.API.Middleware
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


        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
                when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Request was cancelled by the caller.");
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(
                    context,
                    exception);
            }
        }


        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            (int statusCode, string errorCode, string message) =
                exception switch
                {
                    BadRequestException applicationException =>
                        (
                            StatusCodes.Status400BadRequest,
                            applicationException.ErrorCode,
                            applicationException.Message
                        ),


                    UnauthorizedException applicationException =>
                        (
                            StatusCodes.Status401Unauthorized,
                            applicationException.ErrorCode,
                            applicationException.Message
                        ),


                    ForbiddenException applicationException =>
                        (
                            StatusCodes.Status403Forbidden,
                            applicationException.ErrorCode,
                            applicationException.Message
                        ),


                    NotFoundException applicationException =>
                        (
                            StatusCodes.Status404NotFound,
                            applicationException.ErrorCode,
                            applicationException.Message
                        ),


                    ConflictException applicationException =>
                        (
                            StatusCodes.Status409Conflict,
                            applicationException.ErrorCode,
                            applicationException.Message
                        ),


                    ConcurrencyConflictException applicationException =>
                        (
                            StatusCodes.Status409Conflict,
                            applicationException.ErrorCode,
                            applicationException.Message
                        ),


                    DependencyUnavailableException applicationException =>
                        (
                            StatusCodes.Status503ServiceUnavailable,
                            applicationException.ErrorCode,
                            applicationException.Message
                        ),


                    DbUpdateException =>
                        (
                            StatusCodes.Status409Conflict,
                            "DATABASE_UPDATE_CONFLICT",
                            "The requested database operation could not be completed."
                        ),


                    _ =>
                        (
                            StatusCodes.Status500InternalServerError,
                            "INTERNAL_SERVER_ERROR",
                            "An unexpected error occurred."
                        )
                };


            if (statusCode >= 500)
            {
                _logger.LogError(
                    exception,
                    "Unhandled ProductQuote API error.");
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "ProductQuote API request failed with {ErrorCode}.",
                    errorCode);
            }


            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";


            await context.Response.WriteAsJsonAsync(
                new
                {
                    type = $"https://httpstatuses.com/{statusCode}",
                    title = GetTitle(statusCode),
                    status = statusCode,
                    detail = message,
                    errorCode,
                    correlationId =
                        context.TraceIdentifier
                },
                context.RequestAborted);
        }


        private static string GetTitle(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest =>
                    "Bad request",


                StatusCodes.Status401Unauthorized =>
                    "Unauthorized",


                StatusCodes.Status403Forbidden =>
                    "Forbidden",


                StatusCodes.Status404NotFound =>
                    "Not found",


                StatusCodes.Status409Conflict =>
                    "Conflict",


                StatusCodes.Status503ServiceUnavailable =>
                    "Dependency unavailable",


                _ => "Internal server error"
            };
        }
    }
}
