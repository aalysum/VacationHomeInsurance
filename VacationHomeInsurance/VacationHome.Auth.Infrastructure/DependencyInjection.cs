using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VacationHome.Auth.Application.Common;
using VacationHome.Auth.Application.Interfaces.Persistence;
using VacationHome.Auth.Application.Interfaces.Repositories;
using VacationHome.Auth.Application.Interfaces.Services;
using VacationHome.Auth.Domain.Entities;
using VacationHome.Auth.Infrastructure.Authentication;
using VacationHome.Auth.Infrastructure.Clients;
using VacationHome.Auth.Infrastructure.Identity;
using VacationHome.Auth.Infrastructure.Persistence;
using VacationHome.Auth.Infrastructure.Persistence.Context;
using VacationHome.Auth.Infrastructure.Persistence.Repositories;
using VacationHome.Auth.Infrastructure.Security;
using VacationHome.Auth.Infrastructure.Storage;
using VacationHome.Auth.Application.Interfaces.Clients;
using Microsoft.Extensions.Http.Resilience;




namespace VacationHome.Auth.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            AddPersistence(
                services,
                configuration);


            AddIdentity(services);


            AddRepositories(services);


            AddInfrastructureServices(
                services,
                configuration);


            return services;
        }


        private static void AddPersistence(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection");


            if (string.IsNullOrWhiteSpace(
                    connectionString))
            {
                throw new InvalidOperationException(
                    "The DefaultConnection connection string is missing.");
            }


            services.AddDbContext<AuthDbContext>(
                options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        sqlServerOptions =>
                        {
                            sqlServerOptions
                                .EnableRetryOnFailure(
                                    maxRetryCount: 3,
                                    maxRetryDelay:
                                        TimeSpan.FromSeconds(5),
                                    errorNumbersToAdd: null);
                        });
                });
        }


        private static void AddIdentity(
            IServiceCollection services)
        {
            services
                .AddIdentity<
                    ApplicationUser,
                    IdentityRole>(
                    options =>
                    {
                        ConfigurePasswordOptions(
                            options);


                        ConfigureLockoutOptions(
                            options);


                        ConfigureUserOptions(
                            options);
                    })
                .AddEntityFrameworkStores<
                    AuthDbContext>()
                .AddDefaultTokenProviders();


            services.AddHttpContextAccessor();
        }


        private static void ConfigurePasswordOptions(
            IdentityOptions options)
        {
            options.Password.RequiredLength = 8;


            options.Password
                .RequiredUniqueChars = 1;


            options.Password
                .RequireUppercase = true;


            options.Password
                .RequireLowercase = true;


            options.Password
                .RequireDigit = true;


            options.Password
                .RequireNonAlphanumeric = true;
        }


        private static void ConfigureLockoutOptions(
            IdentityOptions options)
        {
            options.Lockout
                .MaxFailedAccessAttempts = 5;


            options.Lockout
                .DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);


            options.Lockout
                .AllowedForNewUsers = true;
        }


        private static void ConfigureUserOptions(
            IdentityOptions options)
        {
            options.User
                .RequireUniqueEmail = true;


            /*
             * Email confirmation is not required for the MVP.
             */
            options.SignIn
                .RequireConfirmedEmail = false;


            options.SignIn
                .RequireConfirmedAccount = false;
        }


        private static void AddRepositories(
            IServiceCollection services)
        {
            services.AddScoped<
                IClaimsManagerApplicationRepository,
                ClaimsManagerApplicationRepository>();


            services.AddScoped<
                IRefreshTokenRepository,
                RefreshTokenRepository>();


            services.AddScoped<
                IPasswordResetTokenRepository,
                PasswordResetTokenRepository>();


            services.AddScoped<
                ILoginAttemptRepository,
                LoginAttemptRepository>();


            services.AddScoped<
                IAuditLogRepository,
                AuditLogRepository>();
        }


        private static void AddInfrastructureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<
                IJwtTokenService,
                JwtTokenService>();


            services.AddHttpContextAccessor();


            services.AddScoped<
                ICurrentUserService,
                CurrentUserService>();


            services.AddScoped<
                IFileStorageService,
                FileStorageService>();


            services.AddScoped<
                IAuthUnitOfWork,
                AuthUnitOfWork>();


            services.AddScoped<
                IInternalTokenService,
                InternalTokenService>();


            services
                .AddOptions<CustomerServiceOptions>()
                .Bind(
                    configuration.GetSection(
                        CustomerServiceOptions.SectionName))
                .Validate(
                    options =>
                        Uri.TryCreate(
                            options.BaseUrl,
                            UriKind.Absolute,
                            out _),
                    "Customer Service BaseUrl is invalid.")
                .Validate(
                    options =>
                        options.TimeoutSeconds > 0,
                    "Customer Service timeout must be positive.")
                .ValidateOnStart();


            services.AddHttpClient<
                ICustomerServiceClient,
                CustomerServiceClient>(
                (serviceProvider, httpClient) =>
                {
                    var options =
                        serviceProvider
                            .GetRequiredService<
                                IOptions<CustomerServiceOptions>>()
                            .Value;


                    httpClient.BaseAddress =
                        new Uri(
                            options.BaseUrl
                                .TrimEnd('/') +
                            "/");


                    httpClient.Timeout =
                        TimeSpan.FromSeconds(
                            options.TimeoutSeconds);
                }).AddStandardResilienceHandler(options =>
                {
                    options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(30);


                    options.CircuitBreaker.FailureRatio = 0.5;


                    options.CircuitBreaker.MinimumThroughput = 10;


                    options.CircuitBreaker.SamplingDuration =
                    TimeSpan.FromSeconds(30);


                    options.CircuitBreaker.BreakDuration =
                    TimeSpan.FromSeconds(15);
                });
        }
    }
}
