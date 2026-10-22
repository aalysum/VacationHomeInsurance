using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VacationHome.ProductQuote.Application.Interfaces.External;
using VacationHome.ProductQuote.Application.Interfaces.Persistence;
using VacationHome.ProductQuote.Application.Interfaces.Repositories;
using VacationHome.ProductQuote.Application.Interfaces.Services;
using VacationHome.ProductQuote.Infrastructure.ExternalServices;
using VacationHome.ProductQuote.Infrastructure.Identity;
using VacationHome.ProductQuote.Infrastructure.Persistence;
using VacationHome.ProductQuote.Infrastructure.Persistence.Context;
using VacationHome.ProductQuote.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Http.Resilience;


namespace VacationHome.ProductQuote.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString =
                configuration.GetConnectionString(
                    "ProductQuoteDb")
                ?? throw new InvalidOperationException(
                    "Connection string 'ProductQuoteDb' was not configured.");


            services.AddDbContext<ProductQuoteDbContext>(
                options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(
                                typeof(ProductQuoteDbContext)
                                    .Assembly
                                    .FullName);


                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay:
                                    TimeSpan.FromSeconds(5),
                                errorNumbersToAdd: null);
                        });
                });


            services.AddScoped<
                ProductQuoteDatabaseSeeder>();


            services.AddScoped<
            ICorrelationContext,
            CorrelationContext>();


            services.AddScoped<
            ICurrentUserService,
            CurrentUserService>();


            services.AddHttpClient<
                ICustomerServiceClient,
                CustomerServiceClient>(
                    client =>
                    {
                        client.BaseAddress =
                            GetRequiredBaseAddress(
                                configuration,
                                "ExternalServices:CustomerService:BaseUrl");
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


            services.AddHttpClient<
                IPolicyServiceClient,
                PolicyServiceClient>(
                    client =>
                    {
                        client.BaseAddress =
                            GetRequiredBaseAddress(
                                configuration,
                                "ExternalServices:PolicyService:BaseUrl");
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


            services.AddTransient<
                InternalServiceAuthenticationHandler>();


            services.AddScoped<
                IAuditLogRepository,
                AuditLogRepository>();


            services.AddScoped<
                ICoverageRepository,
                CoverageRepository>();


            services.AddScoped<
                IPricingConfigurationRepository,
                PricingConfigurationRepository>();


            services.AddScoped<
                IProductCoverageRepository,
                ProductCoverageRepository>();


            services.AddScoped<
                IProductRepository,
                ProductRepository>();


            services.AddScoped<
                IQuoteApprovalHistoryRepository,
                QuoteApprovalHistoryRepository>();


            services.AddScoped<
                IQuoteCoverageRepository,
                QuoteCoverageRepository>();


            services.AddScoped<
                IQuoteRepository,
                QuoteRepository>();


            services.AddScoped<
                IRiskConfigurationRepository,
                RiskConfigurationRepository>();


            services.AddScoped<
                ISequenceRepository,
                SequenceRepository>();


            services.AddScoped<
                IUnitOfWork,
                UnitOfWork>();


            return services;
        }


        private static Uri GetRequiredBaseAddress(
    IConfiguration configuration,
    string configurationKey)
        {
            string baseUrl =
                configuration[configurationKey]
                ?? throw new InvalidOperationException(
                    $"Configuration value '{configurationKey}' was not configured.");


            if (!Uri.TryCreate(
                baseUrl,
                UriKind.Absolute,
                out Uri? baseAddress))
            {
                throw new InvalidOperationException(
                    $"Configuration value '{configurationKey}' must be a valid absolute URL.");
            }


            return baseAddress;
        }
    }
}
