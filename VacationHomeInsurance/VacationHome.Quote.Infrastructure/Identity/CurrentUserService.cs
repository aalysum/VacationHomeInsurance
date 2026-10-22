using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VacationHome.ProductQuote.Application.Interfaces.Services;


namespace VacationHome.ProductQuote.Infrastructure.Identity
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


        public Guid? UserId
        {
            get
            {
                ClaimsPrincipal? user =
                    _httpContextAccessor
                        .HttpContext?
                        .User;


                string? value =
                    user?.FindFirstValue(
                        ClaimTypes.NameIdentifier)
                    ?? user?.FindFirstValue("sub")
                    ?? user?.FindFirstValue("user_id");


                return Guid.TryParse(
                    value,
                    out Guid userId)
                        ? userId
                        : null;
            }
        }


        public string? Role
        {
            get
            {
                ClaimsPrincipal? user =
                    _httpContextAccessor
                        .HttpContext?
                        .User;


                return user?.FindFirstValue(
                           ClaimTypes.Role)
                       ?? user?.FindFirstValue("role");
            }
        }


        public bool IsAuthenticated =>
            _httpContextAccessor
                .HttpContext?
                .User
                .Identity?
                .IsAuthenticated == true;
    }
}
