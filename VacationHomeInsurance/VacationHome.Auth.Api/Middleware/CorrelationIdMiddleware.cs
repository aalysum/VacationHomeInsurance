namespace VacationHome.Auth.API.Middleware;


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
        string correlationId;


        if (
            context.Request.Headers.TryGetValue(
                HeaderName,
                out var values) &&
            !string.IsNullOrWhiteSpace(
                values.FirstOrDefault()))
        {
            correlationId =
                values.First()!;
        }
        else
        {
            correlationId =
                Guid.NewGuid().ToString("N");
        }


        context.TraceIdentifier =
            correlationId;


        context.Request.Headers[HeaderName] =
            correlationId;


        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] =
                correlationId;


            return Task.CompletedTask;
        });


        await _next(context);
    }
}
