using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VacationHome.Customer.Application.Interfaces.Services;
using VacationHome.Customer.Application.Services;
using VacationHome.Customer.Application.Validators;


namespace VacationHome.Customer.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<
                ICustomerService,
                CustomerService>();


            services.AddScoped<
                ICustomerOnboardingService,
                CustomerOnboardingService>();


            services.AddScoped<
                IInternalPropertyService,
                InternalPropertyService>();


            services.AddValidatorsFromAssemblyContaining<
                CreateCustomerRequestValidator>();


            services.AddScoped<
                IAdminCustomerService,
                AdminCustomerService>();


            services.AddScoped<
                IInternalCustomerService,
                InternalCustomerService>();




            return services;
        }
    }
}
