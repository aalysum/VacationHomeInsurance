using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VacationHome.PolicyService.Application
    .Interfaces.Services;


namespace VacationHome.PolicyService
    .Infrastructure.Services
{
    public sealed class CurrentUserService
        : ICurrentUserService
    {
        private readonly IHttpContextAccessor
            _httpContextAccessor;


        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor =
                httpContextAccessor;
        }


        public Guid GetUserId()
        {
            string? value =
                GetClaimValue(
                    "sub",
                    ClaimTypes.NameIdentifier);


            return Guid.TryParse(
                value,
                out Guid id)
                    ? id
                    : Guid.Empty;
        }


        public string GetEmail()
        {
            return GetClaimValue(
                    "email",
                    ClaimTypes.Email)
                ?? string.Empty;
        }


        public bool IsAuthenticated()
        {
            return _httpContextAccessor
                .HttpContext?
                .User?
                .Identity?
                .IsAuthenticated
                == true;
        }


        private string? GetClaimValue(
            string jwtClaimType,
            string mappedClaimType)
        {
            ClaimsPrincipal? user =
                _httpContextAccessor
                    .HttpContext?
                    .User;


            string? value =
                user?.FindFirstValue(
                    jwtClaimType);


            if (string.IsNullOrWhiteSpace(
                value))
            {
                value =
                    user?.FindFirstValue(
                        mappedClaimType);
            }


            return string.IsNullOrWhiteSpace(
                value)
                    ? null
                    : value.Trim();
        }
    }
}
