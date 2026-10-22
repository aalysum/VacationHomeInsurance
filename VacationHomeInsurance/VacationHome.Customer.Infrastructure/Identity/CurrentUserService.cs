using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VacationHome.Customer.Application.Interfaces.Services;


namespace VacationHome.Customer.Infrastructure.Identity
{
    public class CurrentUserService
        : ICurrentUserService
    {
        private const string CorrelationIdHeader =
            "X-Correlation-ID";


        private readonly IHttpContextAccessor
            _httpContextAccessor;


        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor =
                httpContextAccessor;
        }


        public string UserId =>
            GetClaimValue(
                ClaimTypes.NameIdentifier,
                "sub");


        public string Email =>
            GetClaimValue(
                ClaimTypes.Email,
                "email");


        public string Role =>
            GetClaimValue(
                ClaimTypes.Role,
                "role");


        public string CorrelationId
        {
            get
            {
                var context =
                    _httpContextAccessor.HttpContext;


                if (context is null)
                {
                    return string.Empty;
                }


                if (context.Request.Headers.TryGetValue(
                        CorrelationIdHeader,
                        out var correlationId) &&
                    !string.IsNullOrWhiteSpace(
                        correlationId.FirstOrDefault()))
                {
                    return correlationId.First()!;
                }


                return context.TraceIdentifier;
            }
        }


        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?
                .User.Identity?
                .IsAuthenticated
            ?? false;


        private string GetClaimValue(
            string primaryClaimType,
            string fallbackClaimType)
        {
            var principal =
                _httpContextAccessor.HttpContext?.User;


            return principal?
                       .FindFirstValue(primaryClaimType)
                   ?? principal?
                       .FindFirstValue(fallbackClaimType)
                   ?? string.Empty;
        }
    }
}
