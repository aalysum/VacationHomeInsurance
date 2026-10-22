using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VacationHome.Auth.Application.Interfaces.Services;
using VacationHome.Auth.Application.Services;


namespace VacationHome.Auth.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<
                IAuthService,
                AuthService>();


            services.AddValidatorsFromAssembly(
                typeof(DependencyInjection).Assembly);


            return services;
        }
    }
}
