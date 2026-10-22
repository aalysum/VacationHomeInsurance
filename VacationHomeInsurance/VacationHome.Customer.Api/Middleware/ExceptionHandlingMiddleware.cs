using System.Net;
using System.Text.Json;
using FluentValidation;
using VacationHome.Customer.Application.Exceptions;


namespace VacationHome.Customer.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;


        private readonly ILogger<
            ExceptionHandlingMiddleware> _logger;


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
            var correlationId =
                context.TraceIdentifier;


            var error = MapException(
                exception,
                correlationId);


            if (error.StatusCode >= 500)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while processing {Method} {Path}. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);
            }
            else
            {
                _logger.LogWarning(
                    "Request failed with {ErrorCode} while processing {Method} {Path}. CorrelationId: {CorrelationId}",
                    error.Code,
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);
            }


            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "The response has already started. The error response cannot be written. CorrelationId: {CorrelationId}",
                    correlationId);


                return;
            }


            context.Response.Clear();


            context.Response.StatusCode =
                error.StatusCode;


            context.Response.ContentType =
                "application/problem+json";


            var response = new
            {
                type = GetProblemType(
                    error.StatusCode),


                title = error.Title,


                status = error.StatusCode,


                code = error.Code,


                detail = error.Message,


                correlationId,


                errors = error.ValidationErrors
            };


            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    }));
        }


        private static ErrorDetails MapException(
            Exception exception,
            string correlationId)
        {
            return exception switch
            {
                ValidationException validationException =>
                    new ErrorDetails
                    {
                        StatusCode =
                            StatusCodes.Status400BadRequest,


                        Title =
                            "Validation failed",


                        Code =
                            "VALIDATION_ERROR",


                        Message =
                            "One or more validation errors occurred.",


                        ValidationErrors =
                            validationException.Errors
                                .GroupBy(
                                    error =>
                                        error.PropertyName)
                                .ToDictionary(
                                    group => group.Key,
                                    group => group
                                        .Select(
                                            error =>
                                                error.ErrorMessage)
                                        .Distinct()
                                        .ToArray())
                    },


                BadRequestException badRequestException =>
                    CreateApplicationError(
                        StatusCodes.Status400BadRequest,
                        "Bad request",
                        badRequestException),


                UnauthorizedException unauthorizedException =>
                    CreateApplicationError(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        unauthorizedException),


                ForbiddenException forbiddenException =>
                    CreateApplicationError(
                        StatusCodes.Status403Forbidden,
                        "Forbidden",
                        forbiddenException),


                NotFoundException notFoundException =>
                    CreateApplicationError(
                        StatusCodes.Status404NotFound,
                        "Not found",
                        notFoundException),


                ConflictException conflictException =>
                    CreateApplicationError(
                        StatusCodes.Status409Conflict,
                        "Conflict",
                        conflictException),


                _ =>
                    new ErrorDetails
                    {
                        StatusCode =
                            StatusCodes
                                .Status500InternalServerError,


                        Title =
                            "Internal server error",


                        Code =
                            "INTERNAL_ERROR",


                        Message =
                            "An unexpected error occurred. Use the correlation ID when contacting support."
                    }
            };
        }


        private static ErrorDetails CreateApplicationError(
            int statusCode,
            string title,
            ApplicationExceptionBase exception)
        {
            return new ErrorDetails
            {
                StatusCode = statusCode,
                Title = title,
                Code = exception.Code,
                Message = exception.Message
            };
        }


        private static string GetProblemType(
            int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest =>
                    "https://httpstatuses.com/400",


                StatusCodes.Status401Unauthorized =>
                    "https://httpstatuses.com/401",


                StatusCodes.Status403Forbidden =>
                    "https://httpstatuses.com/403",


                StatusCodes.Status404NotFound =>
                    "https://httpstatuses.com/404",


                StatusCodes.Status409Conflict =>
                    "https://httpstatuses.com/409",


                _ =>
                    "https://httpstatuses.com/500"
            };
        }


        private sealed class ErrorDetails
        {
            public int StatusCode { get; init; }


            public string Title { get; init; }
                = string.Empty;


            public string Code { get; init; }
                = string.Empty;


            public string Message { get; init; }
                = string.Empty;


            public Dictionary<string, string[]>?
                ValidationErrors
            { get; init; }
        }
    }
}
