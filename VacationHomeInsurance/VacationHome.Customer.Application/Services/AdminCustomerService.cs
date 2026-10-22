using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Application.DTOs.Admin;
using VacationHome.Customer.Application.DTOs.Properties;
using VacationHome.Customer.Application.Exceptions;
using VacationHome.Customer.Application.Interfaces.Repositories;
using VacationHome.Customer.Application.Interfaces.Services;
using VacationHome.Customer.Application.Mappers;


namespace VacationHome.Customer.Application.Services
{
    public class AdminCustomerService
        : IAdminCustomerService
    {
        private readonly IAdminCustomerQueryRepository
            _repository;


        public AdminCustomerService(
            IAdminCustomerQueryRepository repository)
        {
            _repository = repository;
        }


        public async Task<
            PagedResult<AdminCustomerSummaryResponse>>
            GetCustomersAsync(
                AdminCustomerQueryParameters query,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);


            var result =
                await _repository.GetCustomersAsync(
                    query,
                    cancellationToken);


            return new PagedResult<
                AdminCustomerSummaryResponse>
            {
                Items = result.Items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = result.TotalCount
            };
        }


        public async Task<AdminCustomerDetailsResponse>
            GetCustomerAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            var customer =
                await _repository
                    .GetCustomerWithProfileAsync(
                        customerId,
                        cancellationToken);


            if (customer is null)
            {
                throw new NotFoundException(
                    ErrorCodes.CustomerNotFound,
                    "Customer not found.");
            }


            var propertyCount =
                await _repository.GetPropertyCountAsync(
                    customerId,
                    cancellationToken);


            return new AdminCustomerDetailsResponse
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


                Status =
                    customer.Status,


                City =
                    customer.Profile?.City,


                State =
                    customer.Profile?.State,


                PostalCode =
                    customer.Profile?.PostalCode,


                Country =
                    customer.Profile?.Country,


                PropertyCount =
                    propertyCount,


                CreatedAtUtc =
                    customer.CreatedAtUtc,


                UpdatedAtUtc =
                    customer.Profile?.UpdatedAtUtc
                    ?? customer.UpdatedAtUtc
            };
        }


        public async Task<
            PagedResult<VacationPropertyResponse>>
            GetCustomerPropertiesAsync(
                Guid customerId,
                AdminCustomerPropertyQueryParameters query,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);


            var customerExists =
                await _repository.CustomerExistsAsync(
                    customerId,
                    cancellationToken);


            if (!customerExists)
            {
                throw new NotFoundException(
                    ErrorCodes.CustomerNotFound,
                    "Customer not found.");
            }


            var result =
                await _repository
                    .GetCustomerPropertiesAsync(
                        customerId,
                        query,
                        cancellationToken);


            return new PagedResult<
                VacationPropertyResponse>
            {
                Items = result.Items
                    .Select(
                        VacationPropertyMapper.ToResponse)
                    .ToList(),


                PageNumber =
                    query.PageNumber,


                PageSize =
                    query.PageSize,


                TotalCount =
                    result.TotalCount
            };
        }
    }
}
