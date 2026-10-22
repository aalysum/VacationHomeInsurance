using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VacationHome.ClaimService.Application.Interfaces.Services;


namespace VacationHome.ClaimService.Infrastructure.Identity
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


        public string AuthUserId =>
            GetClaimValue(
                "sub",
                ClaimTypes.NameIdentifier);


        public Guid UserId =>
            Guid.TryParse(
                AuthUserId,
                out Guid userId)
                    ? userId
                    : Guid.Empty;


        public string Email =>
            GetClaimValue(
                "email",
                ClaimTypes.Email);


        public bool IsAuthenticated =>
            _httpContextAccessor
                .HttpContext?
                .User?
                .Identity?
                .IsAuthenticated
            ?? false;


        public string Role =>
            GetClaimValue(
                "role",
                ClaimTypes.Role);


        private string GetClaimValue(
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


            if (string.IsNullOrWhiteSpace(value))
            {
                value =
                    user?.FindFirstValue(
                        mappedClaimType);
            }


            return value?.Trim() ??
                string.Empty;
        }
    }
}
