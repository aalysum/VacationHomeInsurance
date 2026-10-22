using Microsoft.Extensions.Primitives;


namespace VacationHome.ProductQuote.API.Middleware
{
    public sealed class CorrelationIdMiddleware
    {
        public const string HeaderName =
            "X-Correlation-ID";


        private readonly RequestDelegate _next;


        public CorrelationIdMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }


        public async Task InvokeAsync(
            HttpContext context)
        {
            string correlationId =
                ResolveCorrelationId(context);


            context.TraceIdentifier = correlationId;


            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] =
                    correlationId;


                return Task.CompletedTask;
            });


            await _next(context);
        }


        private static string ResolveCorrelationId(
            HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(
                    HeaderName,
                    out StringValues values))
            {
                string? suppliedValue =
                    values.FirstOrDefault();


                if (!string.IsNullOrWhiteSpace(
                        suppliedValue) &&
                    suppliedValue.Length <= 100)
                {
                    return suppliedValue.Trim();
                }
            }


            return Guid.NewGuid().ToString();
        }
    }
}
