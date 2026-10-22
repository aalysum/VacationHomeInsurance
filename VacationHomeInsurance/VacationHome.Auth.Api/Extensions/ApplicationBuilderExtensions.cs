using VacationHome.Auth.API.Middleware;


namespace VacationHome.Auth.API.Extensions
{


    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder
            UseCustomExceptionHandling(
                this IApplicationBuilder app)
        {
            return app.UseMiddleware<
                ExceptionHandlingMiddleware>();
        }
    }
}
