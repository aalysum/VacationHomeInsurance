using Microsoft.Extensions.Logging;
using VacationHome.Auth.Application.Interfaces.Services;


namespace VacationHome.Auth.Infrastructure.ExternalServices
{
    public sealed class
        DevelopmentPasswordResetNotificationService
        : IPasswordResetNotificationService
    {
        private readonly ILogger<
            DevelopmentPasswordResetNotificationService>
            _logger;


        public DevelopmentPasswordResetNotificationService(
            ILogger<
                DevelopmentPasswordResetNotificationService>
                logger)
        {
            _logger = logger;
        }


        public Task SendPasswordResetAsync(
            string email,
            string rawToken,
            CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();


            /*
             * Development-only implementation.
             *
             * Never log raw password-reset tokens in a
             * production environment.
             */
            _logger.LogInformation(
                "Development password-reset token generated " +
                "for {Email}: {Token}",
                email,
                rawToken);


            return Task.CompletedTask;
        }
    }
}
