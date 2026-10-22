using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.External;
using VacationHome.ClaimService.Application.Interfaces.Services;


namespace VacationHome.ClaimService.Application.Services
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
            string authUserId =
                _currentUserService.AuthUserId;


            if (string.IsNullOrWhiteSpace(
                authUserId))
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
            }


            var customer =
                await _customerServiceClient
                    .GetCustomerStatusByAuthUserIdAsync(
                        authUserId,
                        cancellationToken);


            if (customer is null)
            {
                throw new NotFoundException(
                    "Customer account was not found.");
            }


            if (!customer.IsActive)
            {
                throw new ForbiddenException(
                    "The Customer account is inactive.");
            }


            if (customer.CustomerId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Customer Service returned an invalid Customer identifier.");
            }


            return customer.CustomerId;
        }
    }
}
