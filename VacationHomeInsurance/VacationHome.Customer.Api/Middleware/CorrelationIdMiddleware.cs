namespace VacationHome.Customer.API.Middleware
{
    public class CorrelationIdMiddleware
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
            var correlationId =
                GetOrCreateCorrelationId(context);


            context.TraceIdentifier =
                correlationId;


            context.Request.Headers[HeaderName] =
                correlationId;


            context.Response.OnStarting(
                () =>
                {
                    context.Response.Headers[HeaderName] =
                        correlationId;


                    return Task.CompletedTask;
                });


            await _next(context);
        }


        private static string GetOrCreateCorrelationId(
            HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(
                    HeaderName,
                    out var values))
            {
                var suppliedValue =
                    values.FirstOrDefault();


                if (!string.IsNullOrWhiteSpace(
                        suppliedValue))
                {
                    var normalizedValue =
                        suppliedValue.Trim();


                    if (normalizedValue.Length <= 100)
                    {
                        return normalizedValue;
                    }
                }
            }


            return Guid.NewGuid()
                .ToString("N");
        }
    }
}
