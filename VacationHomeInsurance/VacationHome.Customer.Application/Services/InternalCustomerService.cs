using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Application.DTOs.Internal;
using VacationHome.Customer.Application.Exceptions;
using VacationHome.Customer.Application.Interfaces.Repositories;
using VacationHome.Customer.Application.Interfaces.Services;
using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.Services
{
    public class InternalCustomerService
        : IInternalCustomerService
    {
        private readonly ICustomerRepository
            _customerRepository;


        public InternalCustomerService(
            ICustomerRepository customerRepository)
        {
            _customerRepository =
                customerRepository;
        }


        public async Task<CustomerStatusResponse>
            GetCustomerStatusAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            var customer =
                await _customerRepository
                    .GetByIdAsync(
                        customerId,
                        cancellationToken);


            if (customer is null)
            {
                throw new NotFoundException(
                    ErrorCodes.CustomerNotFound,
                    "Customer not found.");
            }


            return MapStatus(
                customer);
        }


        public async Task<CustomerStatusResponse>
            GetCustomerStatusByAuthUserIdAsync(
                string authUserId,
                CancellationToken cancellationToken)
        {
            string normalizedAuthUserId =
                authUserId?.Trim()
                ?? string.Empty;


            if (string.IsNullOrWhiteSpace(
                normalizedAuthUserId))
            {
                throw new BadRequestException(
                    ErrorCodes.InvalidAuthUserId,
                    "Authentication user identifier is required.");
            }


            var customer =
                await _customerRepository
                    .GetByAuthUserIdAsync(
                        normalizedAuthUserId,
                        cancellationToken);


            if (customer is null)
            {
                throw new NotFoundException(
                    ErrorCodes.CustomerNotFound,
                    "Customer not found.");
            }


            return MapStatus(
                customer);
        }


        public async Task<CustomerSummaryDto>
            GetCustomerAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            var customer =
                await _customerRepository
                    .GetByIdAsync(
                        customerId,
                        cancellationToken)
                ?? throw new NotFoundException(
                    ErrorCodes.CustomerNotFound,
                    "Customer was not found.");


            return new CustomerSummaryDto
            {
                CustomerId =
                    customer.Id,

                AuthUserId =
                    customer.AuthUserId,


                FirstName =
                    customer.FirstName,


                LastName =
                    customer.LastName,


                PhoneNumber =
                    customer.PhoneNumber,


                IsActive =
                    customer.Status ==
                    CustomerStatus.Active
            };
        }


        private static CustomerStatusResponse
            MapStatus(
                Domain.Entities.Customers customer)
        {
            return new CustomerStatusResponse
            {
                CustomerId =
                    customer.Id,


                AuthUserId =
                    customer.AuthUserId,


                IsActive =
                    customer.Status ==
                    CustomerStatus.Active
            };
        }
    }
}
