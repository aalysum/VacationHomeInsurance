using System.Net;
using System.Text.Json;
using VacationHome.PolicyService.Application.Exceptions;


namespace VacationHome.PolicyService.API.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;


        public ExceptionHandlingMiddleware(
            RequestDelegate next)
        {
            _next = next;
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


        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var statusCode =
                exception switch
                {
                    NotFoundException =>
                        HttpStatusCode.NotFound,


                    BadRequestException =>
                        HttpStatusCode.BadRequest,


                    ForbiddenException =>
                        HttpStatusCode.Forbidden,


                    UnauthorizedException =>
                        HttpStatusCode.Unauthorized,


                    ConflictException =>
                        HttpStatusCode.Conflict,


                    PaymentAttemptsExceededException =>
                        HttpStatusCode.BadRequest,


                    PolicyIneligibleException =>
                        HttpStatusCode.BadRequest,


                    ConcurrencyConflictException =>
                        HttpStatusCode.Conflict,


                    _ =>
                        HttpStatusCode.InternalServerError
                };


            context.Response.ContentType =
                "application/json";


            context.Response.StatusCode =
                (int)statusCode;


            var response = new
            {
                Success = false,
                Error = exception.Message
            };


            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
