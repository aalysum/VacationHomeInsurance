using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Application.DTOs.Internal;
using VacationHome.Customer.Application.Exceptions;
using VacationHome.Customer.Application.Interfaces.Repositories;
using VacationHome.Customer.Application.Interfaces.Services;
using VacationHome.Customer.Application.Mappers;
using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.Services
{
    public class InternalPropertyService
        : IInternalPropertyService
    {
        private readonly ICustomerRepository
            _customerRepository;


        private readonly IVacationPropertyRepository
            _vacationPropertyRepository;


        public InternalPropertyService(
            ICustomerRepository customerRepository,
            IVacationPropertyRepository vacationPropertyRepository)
        {
            _customerRepository =
                customerRepository;


            _vacationPropertyRepository =
                vacationPropertyRepository;
        }


        public async Task<InternalPropertySnapshotResponse>
            GetPropertySnapshotAsync(
                Guid propertyId,
                CancellationToken cancellationToken)
        {


            var property =
                await _vacationPropertyRepository
                    .GetByIdAsync(
                        propertyId,
                        true,
                        cancellationToken);


            if (property is null)
            {
                throw new NotFoundException(
                    ErrorCodes.PropertyNotFound,
                    "Property not found.");
            }


            if (property.Customer.Status !=
                CustomerStatus.Active)
            {
                throw new ForbiddenException(
                    ErrorCodes.CustomerInactive,
                    "The customer account is inactive.");
            }


            if (property.IsArchived)
            {
                throw new ConflictException(
                    ErrorCodes.PropertyArchived,
                    "The property is archived and cannot be used for a new quote.");
            }


            return VacationPropertyMapper.ToInternalSnapshot(
                property,
                property.Customer);
        }
    }
}
