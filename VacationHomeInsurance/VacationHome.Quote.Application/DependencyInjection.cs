using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VacationHome.ProductQuote.Application.Configuration;
using VacationHome.ProductQuote.Application.Interfaces.Services;
using VacationHome.ProductQuote.Application.Services;


namespace VacationHome.ProductQuote.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<ProductQuoteOptions>()
                .Bind(
                    configuration.GetSection(
                        ProductQuoteOptions.SectionName))
                .Validate(
                    options =>
                        options.QuoteValidityDays > 0,
                    "QuoteValidityDays must be greater than zero.")
                .Validate(
                    options =>
                        options
                            .ApprovedPaymentPeriodDays > 0,
                    "ApprovedPaymentPeriodDays must be greater than zero.")
                .ValidateOnStart();


            services.AddValidatorsFromAssembly(
                typeof(DependencyInjection).Assembly);


            services.AddMemoryCache();


            services.AddScoped<
            IAuditService,
            AuditService>();


            services.AddScoped<
            IConfigurationCacheService,
            ConfigurationCacheService>();


            services.AddScoped<
            INotificationService,
            NotificationService>();


            services.AddScoped<
            IPremiumCalculator,
            PremiumCalculator>();
            services.AddScoped<IProductService, ProductService>();


            services.AddScoped<IQuoteAdminService, QuoteAdminService>();


            services.AddScoped<
                IQuoteGenerationService,
                QuoteGenerationService>();


            services.AddScoped<
                IQuoteIntegrationService,
                QuoteIntegrationService>();


            services.AddScoped<
                IQuoteLifecycleService,
                QuoteLifecycleService>();


            services.AddScoped<
                IQuoteQueryService,
                QuoteQueryService>();


            services.AddScoped<IRiskCalculator, RiskCalculator>();


            services.AddScoped<ConfigurationCacheService, ConfigurationCacheService>();


            return services;
        }
    }
}
