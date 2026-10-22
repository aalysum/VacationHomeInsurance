using VacationHome.PolicyService.Application
    .DTOs.External;
using VacationHome.PolicyService.Application
    .Exceptions;
using VacationHome.PolicyService.Application
    .Interfaces.External;
using VacationHome.PolicyService.Application
    .Interfaces.Services;


namespace VacationHome.PolicyService.Application.Services
{
    public sealed class CustomerIdentityService
        : ICustomerIdentityService
    {
        private readonly ICurrentUserService
            _currentUserService;


        private readonly ICustomerServiceClient
            _customerServiceClient;


        public CustomerIdentityService(
            ICurrentUserService currentUserService,
            ICustomerServiceClient customerServiceClient)
        {
            _currentUserService =
                currentUserService;


            _customerServiceClient =
                customerServiceClient;
        }


        public async Task<Guid>
            GetCurrentCustomerIdAsync(
                CancellationToken cancellationToken)
        {
            if (
                !_currentUserService
                    .IsAuthenticated())
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
            }


            Guid authUserId =
                _currentUserService.GetUserId();


            if (authUserId == Guid.Empty)
            {
                throw new UnauthorizedException(
                    "Authenticated user identifier is missing.");
            }


            CustomerStatusDto customerStatus =
                await _customerServiceClient
                    .GetCustomerStatusAsync(
                        authUserId,
                        cancellationToken);


            if (
                !Guid.TryParse(
                    customerStatus.AuthUserId,
                    out Guid returnedAuthUserId) ||
                returnedAuthUserId != authUserId)
            {
                throw new ForbiddenException(
                    "Customer Service returned a different authentication user identifier.");
            }


            if (!customerStatus.IsActive)
            {
                throw new ForbiddenException(
                    "The Customer account is inactive.");
            }


            if (
                customerStatus.CustomerId ==
                Guid.Empty)
            {
                throw new BadRequestException(
                    "Customer Service returned an invalid Customer identifier.");
            }


            return customerStatus.CustomerId;
        }
    }
}
