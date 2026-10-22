using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VacationHome.ClaimService.Application.Interfaces.Services;
using VacationHome.ClaimService.Application.Services;


namespace VacationHome.ClaimService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(
                typeof(DependencyInjection).Assembly);


            services.AddScoped<IAuditService, AuditService>();


            services.AddScoped<IClaimService, ClaimsService>();


            services.AddScoped<IClaimWorkflowService, ClaimWorkflowService>();


            services.AddScoped<IClaimAdminService, ClaimAdminService>();


            services.AddScoped<IClaimDocumentService, ClaimDocumentService>();


            services.AddScoped<
                ICustomerIdentityService,
                CustomerIdentityService>();




            return services;
        }
    }
}
