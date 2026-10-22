using Microsoft.AspNetCore.Http;
using VacationHome.ProductQuote.Application.Interfaces.Services;


namespace VacationHome.ProductQuote.Infrastructure.Persistence.Context
{
    public sealed class CorrelationContext
        : ICorrelationContext
    {
        public const string HeaderName =
            "X-Correlation-ID";


        private readonly IHttpContextAccessor
            _httpContextAccessor;


        public CorrelationContext(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor =
                httpContextAccessor;
        }


        public string CorrelationId
        {
            get
            {
                HttpContext? httpContext =
                    _httpContextAccessor.HttpContext;


                if (httpContext is null)
                {
                    return Guid.NewGuid().ToString();
                }


                if (httpContext.Items.TryGetValue(
                        HeaderName,
                        out object? itemValue) &&
                    itemValue is string itemCorrelationId &&
                    !string.IsNullOrWhiteSpace(
                        itemCorrelationId))
                {
                    return itemCorrelationId;
                }


                string? headerCorrelationId =
                    httpContext.Request.Headers[
                        HeaderName].FirstOrDefault();


                string correlationId =
                    string.IsNullOrWhiteSpace(
                        headerCorrelationId)
                        ? Guid.NewGuid().ToString()
                        : headerCorrelationId.Trim();


                httpContext.Items[HeaderName] =
                    correlationId;


                return correlationId;
            }
        }
    }
}
