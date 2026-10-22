using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VacationHome.PolicyService.Application.Configuration;
using VacationHome.PolicyService.Application
    .Interfaces.External;
using VacationHome.PolicyService.Application
    .Interfaces.Persistence;
using VacationHome.PolicyService.Application
    .Interfaces.Services;
using VacationHome.PolicyService.Application.Services;
using VacationHome.PolicyService.Infrastructure
    .BackgroundServices;
using VacationHome.PolicyService.Infrastructure.External;
using VacationHome.PolicyService.Infrastructure
    .Persistence;
using VacationHome.PolicyService.Infrastructure
    .Repositories;
using VacationHome.PolicyService.Infrastructure
    .Services;
using Microsoft.Extensions.Http.Resilience;


namespace VacationHome.PolicyService.Infrastructure;


public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString(
                "PolicyDb")
            ?? throw new InvalidOperationException(
                "Connection string 'PolicyDb' was not configured.");


        string quoteServiceBaseUrl =
            configuration[
                "Services:Quote"]
            ?? throw new InvalidOperationException(
                "Services:Quote was not configured.");


        string customerServiceBaseUrl =
            configuration[
                "Services:Customer"]
            ?? throw new InvalidOperationException(
                "Services:Customer was not configured.");


        if (
            !Uri.TryCreate(
                quoteServiceBaseUrl,
                UriKind.Absolute,
                out Uri? quoteServiceBaseAddress))
        {
            throw new InvalidOperationException(
                "Services:Quote must be a valid absolute URL.");
        }


        if (
            !Uri.TryCreate(
                customerServiceBaseUrl,
                UriKind.Absolute,
                out Uri? customerServiceBaseAddress))
        {
            throw new InvalidOperationException(
                "Services:Customer must be a valid absolute URL.");
        }


        services.AddDbContext<PolicyDbContext>(
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


        services.Configure<PolicyDocumentOptions>(
            configuration.GetSection(
                PolicyDocumentOptions.SectionName));


        services.AddHttpContextAccessor();


        services.AddScoped<
            ICurrentUserService,
            CurrentUserService>();


        services.AddScoped<
            INotificationService,
            NotificationService>();


        services.AddScoped<
            IPolicyRepository,
            PolicyRepository>();


        services.AddScoped<
            IPolicyDocumentRepository,
            PolicyDocumentRepository>();


        services.AddScoped<
            IPolicyDocumentService,
            PolicyDocumentService>();


        services.AddScoped<
            IPolicyDocumentFileStore,
            LocalPolicyDocumentFileStore>();


        services.AddSingleton<
            IPolicyPdfGenerator,
            PolicyPdfGenerator>();


        services.AddScoped<
            IPaymentRepository,
            PaymentRepository>();


        services.AddScoped<
            IPolicyRenewalRepository,
            PolicyRenewalRepository>();


        services.AddScoped<
            IAuditLogRepository,
            AuditLogRepository>();


        services.AddScoped<
            ISequenceRepository,
            SequenceRepository>();


        services.AddScoped<
            IAuditService,
            AuditService>();


        services.AddScoped<
            INumberGenerator,
            NumberGenerator>();


        services.AddScoped<
            IPolicyService,
            PoliciesService>();


        services.AddScoped<
            IPaymentService,
            PaymentService>();


        services.AddScoped<
            IRenewalService,
            RenewalService>();


        /*
         * Add this here only if CustomerIdentityService
         * is registered in Infrastructure.
         *
         * If it is already registered in AddApplication(),
         * do not register it again here.
         */
        services.AddScoped<
            ICustomerIdentityService,
            CustomerIdentityService>();


        services.AddHostedService<
            PolicyExpiryBackgroundService>();


        services.AddTransient<
            InternalServiceAuthenticationHandler>();


        services.AddHttpClient<
            IQuoteServiceClient,
            QuoteServiceClient>(
                client =>
                {
                    client.BaseAddress =
                        quoteServiceBaseAddress;


                    client.Timeout =
                        TimeSpan.FromSeconds(30);
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
            ICustomerServiceClient,
            CustomerServiceClient>(
                client =>
                {
                    client.BaseAddress =
                        customerServiceBaseAddress;


                    client.Timeout =
                        TimeSpan.FromSeconds(30);
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


        return services;
    }
}
