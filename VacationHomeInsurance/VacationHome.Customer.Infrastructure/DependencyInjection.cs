using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VacationHome.Customer.Application
    .Interfaces.Repositories;
using VacationHome.Customer.Application
    .Interfaces.Services;
using VacationHome.Customer.Infrastructure.Identity;
using VacationHome.Customer.Infrastructure
    .Persistence;
using VacationHome.Customer.Infrastructure
    .Persistence.Context;
using VacationHome.Customer.Infrastructure
    .Persistence.Repositories;


namespace VacationHome.Customer.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString(
                    "CustomerDb");


            if (string.IsNullOrWhiteSpace(
                    connectionString))
            {
                throw new InvalidOperationException(
                    "The CustomerDb connection string is not configured.");
            }


            services.AddDbContext<CustomerDbContext>(
                options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        sqlServerOptions =>
                        {
                            sqlServerOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay:
                                    TimeSpan.FromSeconds(5),
                                errorNumbersToAdd: null);
                        });
                });


            services.AddHttpContextAccessor();


            services.AddScoped<
                ICurrentUserService,
                CurrentUserService>();


            services.AddScoped<
                ICustomerRepository,
                CustomerRepository>();


            services.AddScoped<
                ICustomerProfileRepository,
                CustomerProfileRepository>();


            services.AddScoped<
                IVacationPropertyRepository,
                VacationPropertyRepository>();


            services.AddScoped<
                IAuditLogRepository,
                AuditLogRepository>();


            services.AddScoped<
                IUnitOfWork,
                UnitOfWork>();


            services.AddScoped<
                ICustomerOnboardingPersistence, CustomerOnboardingPersistence>();


            services.AddScoped<
                IAdminCustomerQueryRepository,
                AdminCustomerQueryRepository>();




            return services;
        }
    }
}
