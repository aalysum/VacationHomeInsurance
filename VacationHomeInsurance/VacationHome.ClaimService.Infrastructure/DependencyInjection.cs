using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VacationHome.ClaimService.Application
    .Interfaces.External;
using VacationHome.ClaimService.Application
    .Interfaces.Persistence;
using VacationHome.ClaimService.Application
    .Interfaces.Services;
using VacationHome.ClaimService.Infrastructure
    .ExternalServices;
using VacationHome.ClaimService.Infrastructure
    .Identity;
using VacationHome.ClaimService.Infrastructure
    .Persistence;
using VacationHome.ClaimService.Infrastructure
    .Persistence.Repositories;
using Microsoft.Extensions.Http.Resilience;


namespace VacationHome.ClaimService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection
            AddInfrastructure(
                this IServiceCollection services,
                IConfiguration configuration)
        {
            string connectionString =
                configuration.GetConnectionString(
                    "ClaimDb")
                ?? throw new InvalidOperationException(
                    "ClaimDb connection string is not configured.");


            services.AddDbContext<ClaimDbContext>(
                options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorNumbersToAdd: null);
                        });
                });


            services.AddHttpContextAccessor();


            services.AddScoped<
                ICurrentUserService,
                CurrentUserService>();


            services.AddScoped<
                IUnitOfWork,
                UnitOfWork>();


            services.AddScoped<
                IClaimRepository,
                ClaimRepository>();


            services.AddScoped<
                IClaimDocumentRepository,
                ClaimDocumentRepository>();


            services.AddScoped<
                IAuditLogRepository,
                AuditLogRepository>();


            services.AddScoped<
                ISequenceRepository,
                SequenceRepository>();


            services.AddScoped<
                IDocumentStorageService,
                LocalDocumentStorageService>();


            /*
             * Short-lived handler because the handler creates
             * service-to-service JWTs for outgoing requests.
             */
            services.AddTransient<
                InternalServiceAuthenticationHandler>();


            AddCustomerServiceClient(
                services,
                configuration);


            AddPolicyServiceClient(
                services,
                configuration);


            AddAuthServiceClient(
                services,
                configuration);


            return services;
        }


        private static void
            AddCustomerServiceClient(
                IServiceCollection services,
                IConfiguration configuration)
        {
            string baseUrl =
                GetRequiredBaseUrl(
                    configuration,
                    "ExternalServices:" +
                    "CustomerService:BaseUrl",
                    "Customer Service");


            services.AddHttpClient<
                ICustomerServiceClient,
                CustomerServiceClient>(
                    client =>
                    {
                        client.BaseAddress =
                            new Uri(
                                baseUrl);


                        client.Timeout =
                            TimeSpan.FromSeconds(
                                30);
                    })
                .AddHttpMessageHandler<
                    InternalServiceAuthenticationHandler>()
                .AddStandardResilienceHandler(options =>
                {
                    options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(30);


                    options.CircuitBreaker.FailureRatio =
                    0.5;


                    options.CircuitBreaker.MinimumThroughput =
                    10;


                    options.CircuitBreaker.SamplingDuration =
                    TimeSpan.FromSeconds(30);


                    options.CircuitBreaker.BreakDuration =
                    TimeSpan.FromSeconds(15);
                    }); 
        }


        private static void
            AddPolicyServiceClient(
                IServiceCollection services,
                IConfiguration configuration)
        {
            string baseUrl =
                GetRequiredBaseUrl(
                    configuration,
                    "ExternalServices:" +
                    "PolicyService:BaseUrl",
                    "Policy Service");


            services.AddHttpClient<
                IPolicyServiceClient,
                PolicyServiceClient>(
                    client =>
                    {
                        client.BaseAddress =
                            new Uri(
                                baseUrl);


                        client.Timeout =
                            TimeSpan.FromSeconds(
                                30);
                    })
                .AddHttpMessageHandler<
                    InternalServiceAuthenticationHandler>()
                .AddStandardResilienceHandler(options =>
                {
                    options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(30);


                    options.CircuitBreaker.FailureRatio =
                    0.5;


                    options.CircuitBreaker.MinimumThroughput =
                    10;


                    options.CircuitBreaker.SamplingDuration =
                    TimeSpan.FromSeconds(30);


                    options.CircuitBreaker.BreakDuration =
                    TimeSpan.FromSeconds(15);
                });
        }


        private static void
            AddAuthServiceClient(
                IServiceCollection services,
                IConfiguration configuration)
        {
            string baseUrl =
                GetRequiredBaseUrl(
                    configuration,
                    "ExternalServices:" +
                    "AuthService:BaseUrl",
                    "Auth Service");


            services.AddHttpClient<
                IAuthServiceClient,
                AuthServiceClient>(
                    client =>
                    {
                        client.BaseAddress =
                            new Uri(
                                baseUrl);


                        client.Timeout =
                            TimeSpan.FromSeconds(
                                30);
                    })
                .AddHttpMessageHandler<
                    InternalServiceAuthenticationHandler>()
                .AddStandardResilienceHandler(options =>
                {
                    options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(30);


                    options.CircuitBreaker.FailureRatio =
                    0.5;


                    options.CircuitBreaker.MinimumThroughput =
                    10;


                    options.CircuitBreaker.SamplingDuration =
                    TimeSpan.FromSeconds(30);


                    options.CircuitBreaker.BreakDuration =
                    TimeSpan.FromSeconds(15);
                }); 
        }


        private static string
            GetRequiredBaseUrl(
                IConfiguration configuration,
                string key,
                string serviceName)
        {
            string? value =
                configuration[key];


            if (
                string.IsNullOrWhiteSpace(
                    value)
            )
            {
                throw new InvalidOperationException(
                    $"{serviceName} URL is not configured at '{key}'.");
            }


            string normalizedValue =
                value.Trim();


            if (
                !Uri.TryCreate(
                    normalizedValue,
                    UriKind.Absolute,
                    out Uri? uri)
            )
            {
                throw new InvalidOperationException(
                    $"{serviceName} URL is invalid.");
            }


            if (
                uri.Scheme != Uri.UriSchemeHttps &&
                uri.Scheme != Uri.UriSchemeHttp
            )
            {
                throw new InvalidOperationException(
                    $"{serviceName} URL must use HTTP or HTTPS.");
            }


            string absoluteUri =
                uri.AbsoluteUri;


            return absoluteUri.EndsWith(
                "/",
                StringComparison.Ordinal)
                    ? absoluteUri
                    : $"{absoluteUri}/";
        }
    }
}
