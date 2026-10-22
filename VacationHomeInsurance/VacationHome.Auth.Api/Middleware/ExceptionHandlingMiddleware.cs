using System.Net;
using System.Text.Json;
using VacationHome.Auth.Application.Exceptions;


namespace VacationHome.Auth.API.Middleware
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
            var statusCode = exception switch
            {
                BadRequestException =>
                    HttpStatusCode.BadRequest,


                UnauthorizedException =>
                    HttpStatusCode.Unauthorized,


                ForbiddenException =>
                    HttpStatusCode.Forbidden,


                NotFoundException =>
                    HttpStatusCode.NotFound,


                _ =>
                    HttpStatusCode.InternalServerError
            };


            var errorCode =
                exception is ApplicationExceptionBase
                    applicationException
                    ? applicationException.Code
                    : "INTERNAL_SERVER_ERROR";


            var message =
                statusCode ==
                HttpStatusCode.InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message;


            if (statusCode ==
                HttpStatusCode.InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while processing {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    "Request failed with code {ErrorCode}. " +
                    "Method: {Method}, Path: {Path}",
                    errorCode,
                    context.Request.Method,
                    context.Request.Path);
            }


            var response = new
            {
                success = false,
                code = errorCode,
                message
            };


            context.Response.ContentType =
                "application/json";


            context.Response.StatusCode =
                (int)statusCode;


            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
