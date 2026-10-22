using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VacationHome.PolicyService.Application.Interfaces.Services;


namespace VacationHome.PolicyService.Infrastructure.BackgroundServices
{
    public sealed class PolicyExpiryBackgroundService
        : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PolicyExpiryBackgroundService> _logger;


        public PolicyExpiryBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<PolicyExpiryBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope =
                        _serviceProvider.CreateScope();


                    var service =
                        scope.ServiceProvider
                            .GetRequiredService<IPolicyService>();


                    await service.ExpirePoliciesAsync(
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "An error occurred while processing policy expirations.");
                }


                try
                {
                    await Task.Delay(
                        TimeSpan.FromHours(1),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
