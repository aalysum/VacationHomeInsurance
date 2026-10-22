using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VacationHome.PolicyService.Application.Interfaces.Services;
using VacationHome.PolicyService.Application.Services;


namespace VacationHome.PolicyService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(
                typeof(DependencyInjection).Assembly);


            services.AddScoped<IAuditService,
                AuditService>();


            services.AddScoped<INumberGenerator,
                NumberGenerator>();


            services.AddScoped<IPolicyService,
                PoliciesService>();


            services.AddScoped<IPaymentService,
                PaymentService>();


            services.AddScoped<IRenewalService,
                RenewalService>();


            services.AddScoped<
                ICustomerIdentityService,
                CustomerIdentityService>();


            return services;
        }
    }
}
